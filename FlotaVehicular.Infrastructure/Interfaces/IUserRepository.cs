using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Infrastructure.Interfaces;

// Acceso a los usuarios del sistema.
public interface IUserRepository
{
    Task<IEnumerable<User>> GetPagedAsync(UserQueryDto query);
    Task<int> CountAsync(UserQueryDto query);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetForUpdateAsync(int id);
    Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
    Task<int> CountActiveAdministratorsAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
