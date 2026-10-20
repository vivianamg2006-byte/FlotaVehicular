using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

public class TechnicianRepository : ITechnicianRepository
{
    private readonly AppDbContext _context;

    public TechnicianRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<Technician> ApplyFilters(IQueryable<Technician> query, TechnicianQueryDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(t => t.FullName.Contains(term)
                || t.IdentificationNumber.Contains(term)
                || t.Email.Contains(term));
        }

        // La desactivación no elimina al técnico del historial.
        if (!filters.IncludeInactive)
            query = query.Where(t => t.IsActive);

        return query;
    }

    private IQueryable<Technician> BaseQuery()
        => _context.Technician
            .AsNoTracking()
            .Include(t => t.TechnicianSpecialties)
                .ThenInclude(ts => ts.Specialty);

    public async Task<IEnumerable<Technician>> GetAllAsync(TechnicianQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(t => t.FullName)
            .ToListAsync();

    public async Task<IEnumerable<Technician>> GetPagedAsync(TechnicianQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(t => t.FullName)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<int> CountAsync(TechnicianQueryDto query)
        => await ApplyFilters(_context.Technician.AsNoTracking(), query).CountAsync();

    public async Task<Technician?> GetByIdAsync(int id)
        => await _context.Technician
            .AsNoTracking()
            .Include(t => t.TechnicianSpecialties)
                .ThenInclude(ts => ts.Specialty)
            .FirstOrDefaultAsync(t => t.IdTechnician == id);

    public async Task<Technician?> GetForUpdateAsync(int id)
        => await _context.Technician
            .FirstOrDefaultAsync(t => t.IdTechnician == id);

    public async Task<bool> ExistsByIdentificationAsync(string identificationNumber, int? excludeId = null)
        => await _context.Technician
            .AnyAsync(t => t.IdentificationNumber == identificationNumber
                && (excludeId == null || t.IdTechnician != excludeId));

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        => await _context.Technician
            .AnyAsync(t => t.Email == email
                && (excludeId == null || t.IdTechnician != excludeId));

    public async Task AddAsync(Technician technician)
    {
        _context.Technician.Add(technician);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
