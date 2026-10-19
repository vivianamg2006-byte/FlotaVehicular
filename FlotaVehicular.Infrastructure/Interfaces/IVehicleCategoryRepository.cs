using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

public interface IVehicleCategoryRepository
{
    Task<IEnumerable<VehicleCategory>> GetAllAsync(string? search);
    Task<IEnumerable<VehicleCategory>> GetPagedAsync(string? search, int pageNumber, int pageSize);
    Task<int> CountAsync(string? search);
    Task<VehicleCategory?> GetByIdAsync(int id);
    Task<VehicleCategory?> GetForUpdateAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> HasVehiclesAsync(int id);
    Task AddAsync(VehicleCategory category);
    void Remove(VehicleCategory category);
    Task SaveChangesAsync();
}
