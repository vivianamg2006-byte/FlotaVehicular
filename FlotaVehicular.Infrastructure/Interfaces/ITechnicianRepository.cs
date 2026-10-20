using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

public interface ITechnicianRepository
{
    Task<IEnumerable<Technician>> GetAllAsync(TechnicianQueryDto query);
    Task<IEnumerable<Technician>> GetPagedAsync(TechnicianQueryDto query);
    Task<int> CountAsync(TechnicianQueryDto query);
    Task<Technician?> GetByIdAsync(int id);
    Task<Technician?> GetForUpdateAsync(int id);
    Task<bool> ExistsByIdentificationAsync(string identificationNumber, int? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    Task<IEnumerable<Technician>> GetAvailableBySpecialtyAsync(int idSpecialty);
    Task AddAsync(Technician technician);
    Task SaveChangesAsync();
}
