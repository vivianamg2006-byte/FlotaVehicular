using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

public interface IMaintenanceOrderRepository
{
    Task<IEnumerable<MaintenanceOrder>> GetPagedAsync(MaintenanceOrderQueryDto query);
    Task<IEnumerable<MaintenanceOrder>> GetAllAsync(MaintenanceOrderQueryDto query);
    Task<int> CountAsync(MaintenanceOrderQueryDto query);
    Task<MaintenanceOrder?> GetByIdAsync(int id);
    Task<Vehicle?> GetVehicleAsync(int idVehicle);
    Task<Specialty?> GetSpecialtyAsync(int idSpecialty);
    Task<int> CountByDateAsync(DateTime date);
    Task<bool> ExistsOrderCodeAsync(string orderCode);
    Task AddAsync(MaintenanceOrder order);
    Task SaveChangesAsync();
}
