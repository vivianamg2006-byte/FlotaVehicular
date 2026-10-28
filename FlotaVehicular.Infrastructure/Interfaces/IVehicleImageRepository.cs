using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

// Acceso a las imágenes de los vehículos y a su archivo en disco.
public interface IVehicleImageRepository
{
    Task<IEnumerable<VehicleImage>> GetByVehicleAsync(int vehicleId);

    Task<IEnumerable<VehicleImage>> GetTrackedByVehicleAsync(int vehicleId);
    Task<VehicleImage?> GetByIdAsync(int vehicleId, int imageId);

    Task<Vehicle?> GetVehicleForUpdateAsync(int vehicleId);
    Task AddAsync(VehicleImage image);
    void Remove(VehicleImage image);
    Task SaveChangesAsync();
}
