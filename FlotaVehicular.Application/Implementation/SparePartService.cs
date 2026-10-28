using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

// Casos de uso de la gestión del catálogo de repuestos.
public class SparePartService : ISparePartService
{
    public const int MaxPageSize = 50;

    private readonly ISparePartRepository _repository;
    private readonly IMapper _mapper;

    public SparePartService(ISparePartRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Lista los repuestos que cumplen el filtro indicado.
    public async Task<Result<IEnumerable<SparePartListDto>>> GetAllAsync(SparePartQueryDto query)
    {
        var spareParts = await _repository.GetAllAsync(query);

        var data = _mapper.Map<IEnumerable<SparePartListDto>>(spareParts);

        return Result<IEnumerable<SparePartListDto>>.Success(data);
    }

    // Lista repuestos con paginación y filtros.
    public async Task<Result<IEnumerable<SparePartListDto>>> GetPagedAsync(SparePartQueryDto query)
    {
        var pagingError = ValidatePaging(query.PageNumber, query.PageSize);
        if (pagingError is not null)
            return Result<IEnumerable<SparePartListDto>>.Failure(400, "Bad Request", pagingError);

        var spareParts = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<SparePartListDto>>(spareParts);

        return Result<IEnumerable<SparePartListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    // Lista los repuestos cuyo stock está por debajo del mínimo.
    public async Task<Result<IEnumerable<SparePartListDto>>> GetBelowMinimumStockAsync(int pageNumber, int pageSize)
    {
        var query = new SparePartQueryDto
        {
            BelowMinimumOnly = true,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return await GetPagedAsync(query);
    }

    // Devuelve el detalle de un repuesto con sus proveedores.
    public async Task<Result<SparePartDetailDto>> GetByIdAsync(int id)
    {
        var sparePart = await _repository.GetByIdAsync(id);

        if (sparePart is null)
            return Result<SparePartDetailDto>.Failure(404, "Not Found", $"El repuesto {id} no existe.");

        return Result<SparePartDetailDto>.Success(_mapper.Map<SparePartDetailDto>(sparePart));
    }

    // Registra un repuesto nuevo.
    public async Task<Result<SparePartDetailDto>> AddAsync(SparePartCreateDto sparePart)
    {
        var validationError = ValidateStock(sparePart.CurrentStock, sparePart.MinStock);
        if (validationError is not null)
            return Result<SparePartDetailDto>.Failure(400, "Bad Request", validationError);

        var code = sparePart.Code!.Trim().ToUpperInvariant();

        if (await _repository.ExistsByCodeAsync(code))
            return Result<SparePartDetailDto>.Failure(409, "Conflict",
                $"Ya existe un repuesto con el código '{code}'.");

        var entity = _mapper.Map<SparePart>(sparePart);
        entity.Code = code;

        await _repository.AddAsync(entity);

        var created = await _repository.GetByIdAsync(entity.IdSparePart);
        var data = _mapper.Map<SparePartDetailDto>(created!);

        return Result<SparePartDetailDto>.Success(data, 201, "Created", "Registro creado.");
    }

    // Actualiza un repuesto existente.
    public async Task<Result<SparePartDetailDto>> UpdateAsync(int id, SparePartUpdateDto sparePart)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<SparePartDetailDto>.Failure(404, "Not Found", $"El repuesto {id} no existe.");

        var validationError = ValidateStock(sparePart.CurrentStock, sparePart.MinStock);
        if (validationError is not null)
            return Result<SparePartDetailDto>.Failure(400, "Bad Request", validationError);

        var code = sparePart.Code!.Trim().ToUpperInvariant();

        if (await _repository.ExistsByCodeAsync(code, id))
            return Result<SparePartDetailDto>.Failure(409, "Conflict",
                $"Ya existe un repuesto con el código '{code}'.");

        _mapper.Map(sparePart, entity);
        entity.Code = code;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<SparePartDetailDto>(updated!);

        return Result<SparePartDetailDto>.Success(data, 200, "OK", "Registro actualizado.");
    }

    // Elimina un repuesto que no tiene proveedores asociados.
    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<int>.Failure(404, "Not Found", $"El repuesto {id} no existe.");

        // Primero deben retirarse las asociaciones del repuesto.
        if (await _repository.HasSuppliersAsync(id))
            return Result<int>.Failure(409, "Conflict",
                $"El repuesto {id} tiene proveedores asociados; primero desasócielos.");

        _repository.Remove(entity);
        await _repository.SaveChangesAsync();

        return Result<int>.Success(id, 200, "OK", "Registro eliminado.");
    }

    private static string? ValidateStock(int currentStock, int minStock)
    {
        if (currentStock < 0)
            return "El stock actual no puede ser negativo.";

        if (minStock < 0)
            return "El stock mínimo no puede ser negativo.";

        return null;
    }

    private static string? ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.";

        return null;
    }
}
