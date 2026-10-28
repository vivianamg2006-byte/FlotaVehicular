using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

// Acceso al catálogo de repuestos y su nivel de stock.
public interface ISparePartRepository
{
    Task<IEnumerable<SparePart>> GetAllAsync(SparePartQueryDto query);
    Task<IEnumerable<SparePart>> GetPagedAsync(SparePartQueryDto query);
    Task<int> CountAsync(SparePartQueryDto query);
    Task<SparePart?> GetByIdAsync(int id);
    Task<SparePart?> GetForUpdateAsync(int id);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    Task<bool> HasSuppliersAsync(int id);
    Task AddAsync(SparePart sparePart);
    void Remove(SparePart sparePart);
    Task SaveChangesAsync();
}
