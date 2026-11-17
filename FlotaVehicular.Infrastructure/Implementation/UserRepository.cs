using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

// Acceso a los usuarios del sistema.
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<User> ApplyFilters(IQueryable<User> query, UserQueryDto filters)
    {
        if (filters.Role.HasValue)
            query = query.Where(u => u.Role == filters.Role.Value);

        if (filters.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filters.IsActive.Value);

        return query;
    }

    private IQueryable<User> BaseQuery()
        => _context.User.AsNoTracking();

    public async Task<IEnumerable<User>> GetPagedAsync(UserQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(u => u.FullName)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<int> CountAsync(UserQueryDto query)
        => await ApplyFilters(_context.User.AsNoTracking(), query).CountAsync();

    public async Task<User?> GetByIdAsync(int id)
        => await _context.User
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUser == id);

    public async Task<User?> GetForUpdateAsync(int id)
        => await _context.User
            .FirstOrDefaultAsync(u => u.IdUser == id);

    // Busca por nombre de usuario o correo y mantiene la entidad trackeada para actualizar el acceso.
    public async Task<User?> GetByLoginForUpdateAsync(string login)
        => await _context.User
            .FirstOrDefaultAsync(u => u.Username == login || u.Email == login);

    public async Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null)
        => await _context.User
            .AnyAsync(u => u.Username == username
                && (excludeId == null || u.IdUser != excludeId));

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        => await _context.User
            .AnyAsync(u => u.Email == email
                && (excludeId == null || u.IdUser != excludeId));

    public async Task<int> CountActiveAdministratorsAsync()
        => await _context.User
            .AsNoTracking()
            .CountAsync(u => u.IsActive && u.Role == UserRole.Administrador);

    public async Task AddAsync(User user)
    {
        _context.User.Add(user);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
