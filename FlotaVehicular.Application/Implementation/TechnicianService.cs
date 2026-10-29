using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

// Casos de uso de la gestión de técnicos.
public class TechnicianService : ITechnicianService
{
    public const int MaxPageSize = 50;

    private readonly ITechnicianRepository _repository;
    private readonly IMapper _mapper;

    public TechnicianService(ITechnicianRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Lista los técnicos que cumplen el filtro indicado.
    public async Task<Result<IEnumerable<TechnicianListDto>>> GetAllAsync(TechnicianQueryDto query)
    {
        var technicians = await _repository.GetAllAsync(query);

        var data = _mapper.Map<IEnumerable<TechnicianListDto>>(technicians);

        return Result<IEnumerable<TechnicianListDto>>.Success(data);
    }

    // Lista técnicos con paginación y filtros.
    public async Task<Result<IEnumerable<TechnicianListDto>>> GetPagedAsync(TechnicianQueryDto query)
    {
        var pagingError = ValidatePaging(query.PageNumber, query.PageSize);
        if (pagingError is not null)
            return Result<IEnumerable<TechnicianListDto>>.Failure(400, "Bad Request", pagingError);

        var technicians = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<TechnicianListDto>>(technicians);

        return Result<IEnumerable<TechnicianListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    // Devuelve el detalle de un técnico con sus especialidades.
    public async Task<Result<TechnicianDetailDto>> GetByIdAsync(int id)
    {
        var technician = await _repository.GetByIdAsync(id);

        if (technician is null)
            return Result<TechnicianDetailDto>.Failure(404, "Not Found", $"El técnico {id} no existe.");

        return Result<TechnicianDetailDto>.Success(_mapper.Map<TechnicianDetailDto>(technician));
    }

    // Registra un técnico nuevo.
    public async Task<Result<TechnicianDetailDto>> AddAsync(TechnicianCreateDto technician)
    {
        var identification = technician.IdentificationNumber!.Trim();
        var email = technician.Email!.Trim();

        if (await _repository.ExistsByIdentificationAsync(identification))
            return Result<TechnicianDetailDto>.Failure(409, "Conflict",
                $"Ya existe un técnico con la identificación '{identification}'.");

        if (await _repository.ExistsByEmailAsync(email))
            return Result<TechnicianDetailDto>.Failure(409, "Conflict",
                $"Ya existe un técnico con el correo '{email}'.");

        var entity = _mapper.Map<Technician>(technician);
        entity.FullName = technician.FullName!.Trim();
        entity.IdentificationNumber = identification;
        entity.Email = email;
        entity.Phone = technician.Phone?.Trim();
        entity.IsActive = true;

        await _repository.AddAsync(entity);

        var data = _mapper.Map<TechnicianDetailDto>(entity);

        return Result<TechnicianDetailDto>.Success(data, 201, "Created", "Registro creado.");
    }

    // Actualiza un técnico existente.
    public async Task<Result<TechnicianDetailDto>> UpdateAsync(int id, TechnicianUpdateDto technician)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<TechnicianDetailDto>.Failure(404, "Not Found", $"El técnico {id} no existe.");

        var identification = technician.IdentificationNumber!.Trim();
        var email = technician.Email!.Trim();

        if (await _repository.ExistsByIdentificationAsync(identification, id))
            return Result<TechnicianDetailDto>.Failure(409, "Conflict",
                $"Ya existe un técnico con la identificación '{identification}'.");

        if (await _repository.ExistsByEmailAsync(email, id))
            return Result<TechnicianDetailDto>.Failure(409, "Conflict",
                $"Ya existe un técnico con el correo '{email}'.");

        entity.FullName = technician.FullName!.Trim();
        entity.IdentificationNumber = identification;
        entity.Email = email;
        entity.Phone = technician.Phone?.Trim();
        entity.IsAvailable = technician.IsAvailable;
        entity.HourlyRate = technician.HourlyRate;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<TechnicianDetailDto>(updated!);

        return Result<TechnicianDetailDto>.Success(data, 200, "OK", "Registro actualizado.");
    }

    // Desactiva un técnico conservando sus especialidades.
    public async Task<Result<TechnicianDetailDto>> DeactivateAsync(int id)
    {
        var technician = await _repository.GetByIdAsync(id);

        if (technician is null)
            return Result<TechnicianDetailDto>.Failure(404, "Not Found", $"El técnico {id} no existe.");

        if (!technician.IsActive && !technician.IsAvailable)
            return Result<TechnicianDetailDto>.Success(_mapper.Map<TechnicianDetailDto>(technician),
                200, "OK", "El técnico ya estaba desactivado.");

        var entity = await _repository.GetForUpdateAsync(id);
        // La baja lógica conserva el historial y retira al técnico de los disponibles.
        entity!.IsActive = false;
        entity.IsAvailable = false;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<TechnicianDetailDto>(updated!);

        return Result<TechnicianDetailDto>.Success(data, 200, "OK", "Técnico desactivado.");
    }

    // Reactiva un técnico desactivado.
    public async Task<Result<TechnicianDetailDto>> ActivateAsync(int id)
    {
        var technician = await _repository.GetByIdAsync(id);

        if (technician is null)
            return Result<TechnicianDetailDto>.Failure(404, "Not Found", $"El técnico {id} no existe.");

        if (technician.IsActive)
            return Result<TechnicianDetailDto>.Success(_mapper.Map<TechnicianDetailDto>(technician),
                200, "OK", "El técnico ya estaba activo.");

        var entity = await _repository.GetForUpdateAsync(id);
        entity!.IsActive = true;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<TechnicianDetailDto>(updated!);

        return Result<TechnicianDetailDto>.Success(data, 200, "OK", "Técnico activado.");
    }

    // Técnicos activos, disponibles y certificados en la especialidad indicada.
    public async Task<Result<IEnumerable<TechnicianListDto>>> GetAvailableBySpecialtyAsync(int idSpecialty)
    {
        var specialists = await _repository.GetAvailableBySpecialtyAsync(idSpecialty);

        var data = _mapper.Map<IEnumerable<TechnicianListDto>>(specialists);

        return Result<IEnumerable<TechnicianListDto>>.Success(data);
    }

    private static string? ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.";

        return null;
    }
}
