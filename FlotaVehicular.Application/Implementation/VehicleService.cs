using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

public class VehicleService : IVehicleService
{
    public const int MaxPageSize = 50;

    private readonly IVehicleRepository _repository;
    private readonly IMapper _mapper;

    public VehicleService(IVehicleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<VehicleListDto>>> GetAllAsync(VehicleQueryDto query)
    {
        var vehicles = await _repository.GetAllAsync(query);

        var data = _mapper.Map<IEnumerable<VehicleListDto>>(vehicles);

        return Result<IEnumerable<VehicleListDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<VehicleListDto>>> GetPagedAsync(VehicleQueryDto query)
    {
        var pagingError = ValidatePaging(query.PageNumber, query.PageSize);
        if (pagingError is not null)
            return Result<IEnumerable<VehicleListDto>>.Failure(400, "Bad Request", pagingError);

        var vehicles = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<VehicleListDto>>(vehicles);

        return Result<IEnumerable<VehicleListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    public async Task<Result<VehicleDetailDto>> GetByIdAsync(int id)
    {
        var vehicle = await _repository.GetByIdAsync(id);

        if (vehicle is null)
            return Result<VehicleDetailDto>.Failure(404, "Not Found", $"El vehículo {id} no existe.");

        return Result<VehicleDetailDto>.Success(_mapper.Map<VehicleDetailDto>(vehicle));
    }

    public async Task<Result<VehicleDetailDto>> AddAsync(VehicleCreateDto vehicle)
    {
        var validationError = await ValidateAsync(vehicle, null);
        if (validationError is not null)
            return Result<VehicleDetailDto>.Failure(validationError.Value.Code, validationError.Value.Title, validationError.Value.Detail);

        var entity = _mapper.Map<Vehicle>(vehicle);
        entity.Code = vehicle.Code!.Trim().ToUpperInvariant();
        entity.LicensePlate = vehicle.LicensePlate!.Trim().ToUpperInvariant();
        entity.Status = VehicleStatus.Operativo;
        entity.CreatedAt = DateTime.UtcNow;
        entity.DeactivationDate = null;

        await _repository.AddAsync(entity);

        var created = await _repository.GetByIdAsync(entity.IdVehicle);
        var data = _mapper.Map<VehicleDetailDto>(created!);

        return Result<VehicleDetailDto>.Success(data, 201, "Created", "Registro creado.");
    }

    public async Task<Result<VehicleDetailDto>> UpdateAsync(int id, VehicleUpdateDto vehicle)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<VehicleDetailDto>.Failure(404, "Not Found", $"El vehículo {id} no existe.");

        if (entity.Status == VehicleStatus.DadoDeBaja)
            return Result<VehicleDetailDto>.Failure(409, "Conflict",
                $"El vehículo {id} está dado de baja y no puede modificarse.");

        var validationError = await ValidateAsync(vehicle, id);
        if (validationError is not null)
            return Result<VehicleDetailDto>.Failure(validationError.Value.Code, validationError.Value.Title, validationError.Value.Detail);

        _mapper.Map(vehicle, entity);
        // Omitir la imagen no debe borrar la que ya tiene el vehículo.
        if (!string.IsNullOrWhiteSpace(vehicle.Image))
            entity.ImagePath = vehicle.Image;

        entity.Code = vehicle.Code!.Trim().ToUpperInvariant();
        entity.LicensePlate = vehicle.LicensePlate!.Trim().ToUpperInvariant();

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<VehicleDetailDto>(updated!);

        return Result<VehicleDetailDto>.Success(data, 200, "OK", "Registro actualizado.");
    }

    public async Task<Result<VehicleDetailDto>> ChangeStatusAsync(int id, VehicleStatusDto status)
    {
        if (status.Status is null)
            return Result<VehicleDetailDto>.Failure(400, "Bad Request", "El estado es obligatorio.");

        // El estado debe pertenecer a la lista definida en VehicleStatus.
        if (!Enum.IsDefined(status.Status.Value))
            return Result<VehicleDetailDto>.Failure(400, "Bad Request",
                $"El valor {(int)status.Status.Value} no corresponde a un estado válido. " +
                "Use: Operativo, EnMantenimiento o FueraDeServicio.");

        if (status.Status == VehicleStatus.DadoDeBaja)
            return Result<VehicleDetailDto>.Failure(400, "Bad Request",
                "Para dar de baja un vehículo use DELETE /api/vehicle/{id} (baja lógica).");

        var entity = await _repository.GetForDeactivationAsync(id);

        if (entity is null)
        {
            var exists = await _repository.GetByIdAsync(id);
            if (exists is null)
                return Result<VehicleDetailDto>.Failure(404, "Not Found", $"El vehículo {id} no existe.");

            return Result<VehicleDetailDto>.Failure(409, "Conflict",
                $"El vehículo {id} ya está dado de baja; su estado no puede cambiarse.");
        }

        entity.Status = status.Status.Value;
        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<VehicleDetailDto>(updated!);

        return Result<VehicleDetailDto>.Success(data, 200, "OK", "Estado actualizado.");
    }

    public async Task<Result<int>> DeactivateAsync(int id)
    {
        var vehicle = await _repository.GetByIdAsync(id);

        if (vehicle is null)
            return Result<int>.Failure(404, "Not Found", $"El vehículo {id} no existe.");

        if (vehicle.Status == VehicleStatus.DadoDeBaja)
            return Result<int>.Success(id, 200, "OK", "El vehículo ya estaba dado de baja.");

        var entity = await _repository.GetForDeactivationAsync(id);

        // La baja conserva el vehículo y sus relaciones.
        entity!.Status = VehicleStatus.DadoDeBaja;
        entity.DeactivationDate = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return Result<int>.Success(id, 200, "OK", "Vehículo dado de baja (registro conservado).");
    }

    private async Task<(int Code, string Title, string Detail)?> ValidateAsync(VehicleCreateDto vehicle, int? excludeId)
        => await ValidateAsync(vehicle.Code, vehicle.LicensePlate, vehicle.IdVehicleCategory,
            vehicle.AcquisitionDate, vehicle.CurrentMileage, vehicle.LastPreventiveMaintenanceMileage, excludeId);

    private async Task<(int Code, string Title, string Detail)?> ValidateAsync(VehicleUpdateDto vehicle, int? excludeId)
        => await ValidateAsync(vehicle.Code, vehicle.LicensePlate, vehicle.IdVehicleCategory,
            vehicle.AcquisitionDate, vehicle.CurrentMileage, vehicle.LastPreventiveMaintenanceMileage, excludeId);

    private async Task<(int Code, string Title, string Detail)?> ValidateAsync(
        string? code, string? licensePlate, int idCategory, DateTime? acquisitionDate,
        int currentMileage, int lastPreventiveMileage, int? excludeId)
    {
        if (!await _repository.CategoryExistsAsync(idCategory))
            return (400, "Bad Request", $"La categoría {idCategory} no existe.");

        if (acquisitionDate.HasValue && acquisitionDate.Value.Date > DateTime.Today)
            return (400, "Bad Request", "La fecha de adquisición no puede ser futura.");

        // El último mantenimiento no puede tener más kilómetros que el vehículo.
        if (lastPreventiveMileage > currentMileage)
            return (400, "Bad Request",
                "El kilometraje del último mantenimiento preventivo no puede superar el kilometraje actual.");

        if (await _repository.ExistsByCodeAsync(code!.Trim().ToUpperInvariant(), excludeId))
            return (409, "Conflict", $"Ya existe un vehículo con el código '{code}'.");

        if (await _repository.ExistsByLicensePlateAsync(licensePlate!.Trim().ToUpperInvariant(), excludeId))
            return (409, "Conflict", $"Ya existe un vehículo con la placa '{licensePlate}'.");

        return null;
    }

    private static string? ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.";

        return null;
    }
}
