using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

public class VehicleCategoryService : IVehicleCategoryService
{
    public const int MaxPageSize = 50;

    private readonly IVehicleCategoryRepository _repository;
    private readonly IMapper _mapper;

    public VehicleCategoryService(IVehicleCategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<VehicleCategoryListDto>>> GetAllAsync(string? search)
    {
        var categories = await _repository.GetAllAsync(search?.Trim());

        var data = _mapper.Map<IEnumerable<VehicleCategoryListDto>>(categories);

        return Result<IEnumerable<VehicleCategoryListDto>>.Success(data);
    }

    public async Task<Result<IEnumerable<VehicleCategoryListDto>>> GetPagedAsync(VehicleCategoryQueryDto query)
    {
        var error = ValidatePaging(query.PageNumber, query.PageSize);
        if (error is not null)
            return Result<IEnumerable<VehicleCategoryListDto>>.Failure(400, "Bad Request", error);

        var categories = await _repository.GetPagedAsync(query.Search?.Trim(), query.PageNumber, query.PageSize);
        var totalRecords = await _repository.CountAsync(query.Search?.Trim());

        var data = _mapper.Map<IEnumerable<VehicleCategoryListDto>>(categories);

        return Result<IEnumerable<VehicleCategoryListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    public async Task<Result<VehicleCategoryDetailDto>> GetByIdAsync(int id)
    {
        var category = await _repository.GetByIdAsync(id);

        if (category is null)
            return Result<VehicleCategoryDetailDto>.Failure(404, "Not Found", $"La categoría {id} no existe.");

        return Result<VehicleCategoryDetailDto>.Success(_mapper.Map<VehicleCategoryDetailDto>(category));
    }

    public async Task<Result<VehicleCategoryDetailDto>> AddAsync(VehicleCategoryCreateDto category)
    {
        var name = category.Name!.Trim();

        if (await _repository.ExistsByNameAsync(name))
            return Result<VehicleCategoryDetailDto>.Failure(409, "Conflict",
                $"Ya existe una categoría con el nombre '{name}'.");

        var entity = _mapper.Map<VehicleCategory>(category);
        entity.Name = name;

        await _repository.AddAsync(entity);

        var data = _mapper.Map<VehicleCategoryDetailDto>(entity);

        return Result<VehicleCategoryDetailDto>.Success(data, 201, "Created", "Registro creado.");
    }

    public async Task<Result<VehicleCategoryDetailDto>> UpdateAsync(int id, VehicleCategoryUpdateDto category)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<VehicleCategoryDetailDto>.Failure(404, "Not Found", $"La categoría {id} no existe.");

        var name = category.Name!.Trim();

        if (await _repository.ExistsByNameAsync(name, id))
            return Result<VehicleCategoryDetailDto>.Failure(409, "Conflict",
                $"Ya existe una categoría con el nombre '{name}'.");

        _mapper.Map(category, entity);
        entity.Name = name;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);

        return Result<VehicleCategoryDetailDto>.Success(_mapper.Map<VehicleCategoryDetailDto>(updated!),
            200, "OK", "Registro actualizado.");
    }

    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<int>.Failure(404, "Not Found", $"La categoría {id} no existe.");

        // También cuentan los vehículos dados de baja.
        if (await _repository.HasVehiclesAsync(id))
            return Result<int>.Failure(409, "Conflict",
                $"La categoría {id} tiene vehículos asociados y no puede eliminarse.");

        _repository.Remove(entity);
        await _repository.SaveChangesAsync();

        return Result<int>.Success(id, 200, "OK", "Registro eliminado.");
    }

    private static string? ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.";

        return null;
    }
}
