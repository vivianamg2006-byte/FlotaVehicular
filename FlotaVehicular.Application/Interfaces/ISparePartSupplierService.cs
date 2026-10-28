using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

// Casos de uso de las ofertas de compra entre repuestos y proveedores.
public interface ISparePartSupplierService
{
    Task<Result<IEnumerable<SparePartSupplierListDto>>> GetAllAsync(SparePartSupplierQueryDto query);
    Task<Result<IEnumerable<SparePartSupplierListDto>>> GetPagedAsync(SparePartSupplierQueryDto query);
    Task<Result<SparePartSupplierListDto>> GetByIdAsync(int idSparePart, int idSupplier);
    Task<Result<SparePartSupplierListDto>> AddAsync(SparePartSupplierCreateDto link);
    Task<Result<SparePartSupplierListDto>> UpdateAsync(int idSparePart, int idSupplier, SparePartSupplierUpdateDto link);
    Task<Result<int>> DeleteAsync(int idSparePart, int idSupplier);
}
