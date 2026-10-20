using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

public interface ITechnicianSpecialtyRepository
{
    Task<IEnumerable<TechnicianSpecialty>> GetAllAsync(TechnicianSpecialtyQueryDto query);
    Task<IEnumerable<TechnicianSpecialty>> GetPagedAsync(TechnicianSpecialtyQueryDto query);
    Task<int> CountAsync(TechnicianSpecialtyQueryDto query);
    Task<TechnicianSpecialty?> GetByIdAsync(int idTechnician, int idSpecialty);
    Task<TechnicianSpecialty?> GetForUpdateAsync(int idTechnician, int idSpecialty);
    Task<bool> ExistsAsync(int idTechnician, int idSpecialty);
    Task<Technician?> GetTechnicianAsync(int idTechnician);
    Task<Specialty?> GetSpecialtyAsync(int idSpecialty);
    Task AddAsync(TechnicianSpecialty link);
    void Remove(TechnicianSpecialty link);
    Task SaveChangesAsync();
}
