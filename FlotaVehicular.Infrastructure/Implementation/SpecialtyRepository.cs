using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

// Acceso a las especialidades de mantenimiento.
public class SpecialtyRepository : ISpecialtyRepository
{
    private readonly AppDbContext _context;

    public SpecialtyRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<Specialty> ApplyFilters(IQueryable<Specialty> query, SpecialtyQueryDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(s => s.Name.Contains(term));
        }

        return query;
    }

    private IQueryable<Specialty> BaseQuery()
        => _context.Specialty
            .AsNoTracking()
            .Include(s => s.TechnicianSpecialties)
                .ThenInclude(ts => ts.Technician);

    public async Task<IEnumerable<Specialty>> GetAllAsync(SpecialtyQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<IEnumerable<Specialty>> GetPagedAsync(SpecialtyQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(s => s.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<int> CountAsync(SpecialtyQueryDto query)
        => await ApplyFilters(_context.Specialty.AsNoTracking(), query).CountAsync();

    public async Task<Specialty?> GetByIdAsync(int id)
        => await _context.Specialty
            .AsNoTracking()
            .Include(s => s.TechnicianSpecialties)
                .ThenInclude(ts => ts.Technician)
            .FirstOrDefaultAsync(s => s.IdSpecialty == id);

    public async Task<Specialty?> GetForUpdateAsync(int id)
        => await _context.Specialty
            .FirstOrDefaultAsync(s => s.IdSpecialty == id);

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        => await _context.Specialty
            .AnyAsync(s => s.Name == name
                && (excludeId == null || s.IdSpecialty != excludeId));

    public async Task<bool> HasTechniciansAsync(int id)
        => await _context.TechnicianSpecialty.AnyAsync(ts => ts.IdSpecialty == id);

    public async Task AddAsync(Specialty specialty)
    {
        _context.Specialty.Add(specialty);
        await _context.SaveChangesAsync();
    }

    public void Remove(Specialty specialty)
        => _context.Specialty.Remove(specialty);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
