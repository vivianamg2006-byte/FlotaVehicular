using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

public class TechnicianSpecialtyRepository : ITechnicianSpecialtyRepository
{
    private readonly AppDbContext _context;

    public TechnicianSpecialtyRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<TechnicianSpecialty> ApplyFilters(IQueryable<TechnicianSpecialty> query, TechnicianSpecialtyQueryDto filters)
    {
        if (filters.IdTechnician.HasValue)
            query = query.Where(ts => ts.IdTechnician == filters.IdTechnician.Value);

        if (filters.IdSpecialty.HasValue)
            query = query.Where(ts => ts.IdSpecialty == filters.IdSpecialty.Value);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(ts => ts.Technician.FullName.Contains(term)
                || ts.Specialty.Name.Contains(term));
        }

        return query;
    }

    private IQueryable<TechnicianSpecialty> BaseQuery()
        => _context.TechnicianSpecialty
            .AsNoTracking()
            .Include(ts => ts.Technician)
            .Include(ts => ts.Specialty);

    public async Task<IEnumerable<TechnicianSpecialty>> GetAllAsync(TechnicianSpecialtyQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(ts => ts.Technician.FullName)
            .ThenBy(ts => ts.Specialty.Name)
            .ToListAsync();

    public async Task<IEnumerable<TechnicianSpecialty>> GetPagedAsync(TechnicianSpecialtyQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(ts => ts.Technician.FullName)
            .ThenBy(ts => ts.Specialty.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<int> CountAsync(TechnicianSpecialtyQueryDto query)
        => await ApplyFilters(_context.TechnicianSpecialty.AsNoTracking(), query).CountAsync();

    public async Task<TechnicianSpecialty?> GetByIdAsync(int idTechnician, int idSpecialty)
        => await _context.TechnicianSpecialty
            .AsNoTracking()
            .Include(ts => ts.Technician)
            .Include(ts => ts.Specialty)
            .FirstOrDefaultAsync(ts => ts.IdTechnician == idTechnician && ts.IdSpecialty == idSpecialty);

    public async Task<TechnicianSpecialty?> GetForUpdateAsync(int idTechnician, int idSpecialty)
        => await _context.TechnicianSpecialty
            .FirstOrDefaultAsync(ts => ts.IdTechnician == idTechnician && ts.IdSpecialty == idSpecialty);

    public async Task<bool> ExistsAsync(int idTechnician, int idSpecialty)
        => await _context.TechnicianSpecialty
            .AnyAsync(ts => ts.IdTechnician == idTechnician && ts.IdSpecialty == idSpecialty);

    public async Task<Technician?> GetTechnicianAsync(int idTechnician)
        => await _context.Technician.AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdTechnician == idTechnician);

    public async Task<Specialty?> GetSpecialtyAsync(int idSpecialty)
        => await _context.Specialty.AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdSpecialty == idSpecialty);

    public async Task AddAsync(TechnicianSpecialty link)
    {
        _context.TechnicianSpecialty.Add(link);
        await _context.SaveChangesAsync();
    }

    public void Remove(TechnicianSpecialty link)
        => _context.TechnicianSpecialty.Remove(link);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
