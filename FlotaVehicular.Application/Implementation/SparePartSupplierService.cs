using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

// Casos de uso de las ofertas de compra entre repuestos y proveedores.
public class SparePartSupplierService : ISparePartSupplierService
{
    public const int MaxPageSize = 50;

    private readonly ISparePartSupplierRepository _repository;
    private readonly IMapper _mapper;

    public SparePartSupplierService(ISparePartSupplierRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    // Lista las ofertas que cumplen el filtro indicado.
    public async Task<Result<IEnumerable<SparePartSupplierListDto>>> GetAllAsync(SparePartSupplierQueryDto query)
    {
        var links = await _repository.GetAllAsync(query);

        var data = _mapper.Map<IEnumerable<SparePartSupplierListDto>>(links);

        return Result<IEnumerable<SparePartSupplierListDto>>.Success(data);
    }

    // Lista ofertas con paginación y filtros.
    public async Task<Result<IEnumerable<SparePartSupplierListDto>>> GetPagedAsync(SparePartSupplierQueryDto query)
    {
        var pagingError = ValidatePaging(query.PageNumber, query.PageSize);
        if (pagingError is not null)
            return Result<IEnumerable<SparePartSupplierListDto>>.Failure(400, "Bad Request", pagingError);

        var links = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<SparePartSupplierListDto>>(links);

        return Result<IEnumerable<SparePartSupplierListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    // Devuelve el detalle de una oferta de repuesto-proveedor.
    public async Task<Result<SparePartSupplierListDto>> GetByIdAsync(int idSparePart, int idSupplier)
    {
        var link = await _repository.GetByIdAsync(idSparePart, idSupplier);

        if (link is null)
            return Result<SparePartSupplierListDto>.Failure(404, "Not Found",
                $"La asociación repuesto {idSparePart} – proveedor {idSupplier} no existe.");

        return Result<SparePartSupplierListDto>.Success(_mapper.Map<SparePartSupplierListDto>(link));
    }

    // Registra una oferta entre un repuesto y un proveedor.
    public async Task<Result<SparePartSupplierListDto>> AddAsync(SparePartSupplierCreateDto link)
    {
        var referenceError = await ValidateReferencesAsync(link.IdSparePart, link.IdSupplier);
        if (referenceError is not null)
            return Result<SparePartSupplierListDto>.Failure(referenceError.Value.Code,
                referenceError.Value.Title, referenceError.Value.Detail);

        // Cada pareja de repuesto y proveedor se registra una sola vez.
        if (await _repository.ExistsAsync(link.IdSparePart, link.IdSupplier))
            return Result<SparePartSupplierListDto>.Failure(409, "Conflict",
                $"El repuesto {link.IdSparePart} ya está asociado al proveedor {link.IdSupplier}.");

        var entity = _mapper.Map<SparePartSupplier>(link);
        entity.AssociatedAt = DateTime.UtcNow;

        await _repository.AddAsync(entity);

        var created = await _repository.GetByIdAsync(entity.IdSparePart, entity.IdSupplier);
        var data = _mapper.Map<SparePartSupplierListDto>(created!);

        return Result<SparePartSupplierListDto>.Success(data, 201, "Created", "Asociación creada.");
    }

    // Actualiza las condiciones de una oferta.
    public async Task<Result<SparePartSupplierListDto>> UpdateAsync(int idSparePart, int idSupplier, SparePartSupplierUpdateDto link)
    {
        var entity = await _repository.GetForUpdateAsync(idSparePart, idSupplier);

        if (entity is null)
            return Result<SparePartSupplierListDto>.Failure(404, "Not Found",
                $"La asociación repuesto {idSparePart} – proveedor {idSupplier} no existe.");

        entity.OfferedPrice = link.OfferedPrice;
        entity.EstimatedDeliveryDays = link.EstimatedDeliveryDays;

        await _repository.SaveChangesAsync();

        var updated = await _repository.GetByIdAsync(idSparePart, idSupplier);
        var data = _mapper.Map<SparePartSupplierListDto>(updated!);

        return Result<SparePartSupplierListDto>.Success(data, 200, "OK", "Condiciones actualizadas.");
    }

    // Elimina la oferta entre repuesto y proveedor.
    public async Task<Result<int>> DeleteAsync(int idSparePart, int idSupplier)
    {
        var entity = await _repository.GetForUpdateAsync(idSparePart, idSupplier);

        if (entity is null)
            return Result<int>.Failure(404, "Not Found",
                $"La asociación repuesto {idSparePart} – proveedor {idSupplier} no existe.");

        _repository.Remove(entity);
        await _repository.SaveChangesAsync();

        return Result<int>.Success(idSparePart, 200, "OK",
            $"Repuesto {idSparePart} desasociado del proveedor {idSupplier}.");
    }

    private async Task<(int Code, string Title, string Detail)?> ValidateReferencesAsync(int idSparePart, int idSupplier)
    {
        if (!await _repository.SparePartExistsAsync(idSparePart))
            return (404, "Not Found", $"El repuesto {idSparePart} no existe.");

        var supplier = await _repository.GetSupplierAsync(idSupplier);

        if (supplier is null)
            return (404, "Not Found", $"El proveedor {idSupplier} no existe.");

        if (!supplier.Status)
            return (400, "Bad Request",
                $"El proveedor {idSupplier} está desactivado; reactivelo antes de asociarlo.");

        return null;
    }

    private static string? ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.";

        return null;
    }
}
