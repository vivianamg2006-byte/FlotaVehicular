using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

public interface ISpecialtyRepository
{
    Task<IEnumerable<Specialty>> GetAllAsync(SpecialtyQueryDto query);
    Task<IEnumerable<Specialty>> GetPagedAsync(SpecialtyQueryDto query);
    Task<int> CountAsync(SpecialtyQueryDto query);
    Task<Specialty?> GetByIdAsync(int id);
    Task<Specialty?> GetForUpdateAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> HasTechniciansAsync(int id);
    Task AddAsync(Specialty specialty);
    void Remove(Specialty specialty);
    Task SaveChangesAsync();
}
