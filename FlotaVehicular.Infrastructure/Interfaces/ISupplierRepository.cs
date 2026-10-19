using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync(SupplierQueryDto query);
    Task<IEnumerable<Supplier>> GetPagedAsync(SupplierQueryDto query);
    Task<int> CountAsync(SupplierQueryDto query);
    Task<Supplier?> GetByIdAsync(int id);
    Task<Supplier?> GetForUpdateAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> HasSparePartsAsync(int id);
    Task AddAsync(Supplier supplier);
    void Remove(Supplier supplier);
    Task SaveChangesAsync();
}
