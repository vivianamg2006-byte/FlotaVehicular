using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

public interface ISparePartSupplierRepository
{
    Task<IEnumerable<SparePartSupplier>> GetAllAsync(SparePartSupplierQueryDto query);
    Task<IEnumerable<SparePartSupplier>> GetPagedAsync(SparePartSupplierQueryDto query);
    Task<int> CountAsync(SparePartSupplierQueryDto query);
    Task<SparePartSupplier?> GetByIdAsync(int idSparePart, int idSupplier);

    Task<SparePartSupplier?> GetForUpdateAsync(int idSparePart, int idSupplier);
    Task<bool> ExistsAsync(int idSparePart, int idSupplier);
    Task<bool> SparePartExistsAsync(int idSparePart);

    Task<Supplier?> GetSupplierAsync(int idSupplier);
    Task AddAsync(SparePartSupplier link);
    void Remove(SparePartSupplier link);
    Task SaveChangesAsync();
}
