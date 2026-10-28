using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

// Acceso a los vehículos, sus categorías y sus condiciones de mantenimiento.
public interface IVehicleRepository
{
    Task<IEnumerable<Vehicle>> GetAllAsync(VehicleQueryDto query);
    Task<IEnumerable<Vehicle>> GetPagedAsync(VehicleQueryDto query);
    Task<int> CountAsync(VehicleQueryDto query);
    Task<Vehicle?> GetByIdAsync(int id);
    Task<Vehicle?> GetForUpdateAsync(int id);
    Task<Vehicle?> GetForDeactivationAsync(int id);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    Task<bool> ExistsByLicensePlateAsync(string licensePlate, int? excludeId = null);
    Task<bool> CategoryExistsAsync(int idVehicleCategory);
    Task AddAsync(Vehicle vehicle);
    Task SaveChangesAsync();
}
