using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

// Casos de uso de la gestión de proveedores.
public class SupplierService : ISupplierService
{
    public const int MaxPageSize = 50;

    private readonly ISupplierRepository _repository;
    private readonly IMapper _mapper;

    public SupplierService(ISupplierRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Lista los proveedores que cumplen el filtro indicado.
    public async Task<Result<IEnumerable<SupplierListDto>>> GetAllAsync(SupplierQueryDto query)
    {
        var suppliers = await _repository.GetAllAsync(query);

        var data = _mapper.Map<IEnumerable<SupplierListDto>>(suppliers);

        return Result<IEnumerable<SupplierListDto>>.Success(data);
    }

    // Lista proveedores con paginación y búsqueda.
    public async Task<Result<IEnumerable<SupplierListDto>>> GetPagedAsync(SupplierQueryDto query)
    {
        var pagingError = ValidatePaging(query.PageNumber, query.PageSize);
        if (pagingError is not null)
            return Result<IEnumerable<SupplierListDto>>.Failure(400, "Bad Request", pagingError);

        var suppliers = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<SupplierListDto>>(suppliers);

        return Result<IEnumerable<SupplierListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    // Devuelve el detalle de un proveedor con sus repuestos.
    public async Task<Result<SupplierDetailDto>> GetByIdAsync(int id)
    {
        var supplier = await _repository.GetByIdAsync(id);

        if (supplier is null)
            return Result<SupplierDetailDto>.Failure(404, "Not Found", $"El proveedor {id} no existe.");

        return Result<SupplierDetailDto>.Success(_mapper.Map<SupplierDetailDto>(supplier));
    }

    // Registra un proveedor nuevo.
    public async Task<Result<SupplierDetailDto>> AddAsync(SupplierCreateDto supplier)
    {
        var name = supplier.Name!.Trim();

        if (await _repository.ExistsByNameAsync(name))
            return Result<SupplierDetailDto>.Failure(409, "Conflict",
                $"Ya existe un proveedor con el nombre '{name}'.");

        var entity = _mapper.Map<Supplier>(supplier);
        entity.Name = name;
        entity.Contact = supplier.Contact!.Trim();
        entity.Status = true;

        await _repository.AddAsync(entity);

        var data = _mapper.Map<SupplierDetailDto>(entity);

        return Result<SupplierDetailDto>.Success(data, 201, "Created", "Registro creado.");
    }

    // Actualiza un proveedor existente.
    public async Task<Result<SupplierDetailDto>> UpdateAsync(int id, SupplierUpdateDto supplier)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<SupplierDetailDto>.Failure(404, "Not Found", $"El proveedor {id} no existe.");

        var name = supplier.Name!.Trim();

        if (await _repository.ExistsByNameAsync(name, id))
            return Result<SupplierDetailDto>.Failure(409, "Conflict",
                $"Ya existe un proveedor con el nombre '{name}'.");

        _mapper.Map(supplier, entity);
        entity.Name = name;
        entity.Contact = supplier.Contact!.Trim();

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<SupplierDetailDto>(updated!);

        return Result<SupplierDetailDto>.Success(data, 200, "OK", "Registro actualizado.");
    }

    // Desactiva un proveedor conservando sus repuestos.
    public async Task<Result<SupplierDetailDto>> DeactivateAsync(int id)
    {
        var supplier = await _repository.GetByIdAsync(id);

        if (supplier is null)
            return Result<SupplierDetailDto>.Failure(404, "Not Found", $"El proveedor {id} no existe.");

        if (!supplier.Status)
            return Result<SupplierDetailDto>.Success(_mapper.Map<SupplierDetailDto>(supplier),
                200, "OK", "El proveedor ya estaba desactivado.");

        var entity = await _repository.GetForUpdateAsync(id);
        // Se desactiva para conservar las relaciones con los repuestos.
        entity!.Status = false;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<SupplierDetailDto>(updated!);

        return Result<SupplierDetailDto>.Success(data, 200, "OK",
            "Proveedor desactivado; sus asociaciones se conservan.");
    }

    // Reactiva un proveedor desactivado.
    public async Task<Result<SupplierDetailDto>> ActivateAsync(int id)
    {
        var supplier = await _repository.GetByIdAsync(id);

        if (supplier is null)
            return Result<SupplierDetailDto>.Failure(404, "Not Found", $"El proveedor {id} no existe.");

        if (supplier.Status)
            return Result<SupplierDetailDto>.Success(_mapper.Map<SupplierDetailDto>(supplier),
                200, "OK", "El proveedor ya estaba activo.");

        var entity = await _repository.GetForUpdateAsync(id);
        entity!.Status = true;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(id);
        var data = _mapper.Map<SupplierDetailDto>(updated!);

        return Result<SupplierDetailDto>.Success(data, 200, "OK", "Proveedor activado.");
    }

    // Elimina un proveedor que no tiene repuestos asociados.
    public async Task<Result<int>> DeleteAsync(int id)
    {
        var entity = await _repository.GetForUpdateAsync(id);

        if (entity is null)
            return Result<int>.Failure(404, "Not Found", $"El proveedor {id} no existe.");

        // Un proveedor con asociaciones se puede desactivar, pero no borrar.
        if (await _repository.HasSparePartsAsync(id))
            return Result<int>.Failure(409, "Conflict",
                $"El proveedor {id} tiene repuestos asociados; para conservar la integridad use la desactivación (PUT /api/supplier/{id}/deactivate).");

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
