using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

public class VehicleCategoryRepository : IVehicleCategoryRepository
{
    private readonly AppDbContext _context;

    public VehicleCategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<VehicleCategory> Query(string? search)
    {
        IQueryable<VehicleCategory> query = _context.VehicleCategory
            .AsNoTracking()
            .Include(c => c.Vehicles);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Name.Contains(term) || (c.Description != null && c.Description.Contains(term)));
        }

        return query;
    }

    public async Task<IEnumerable<VehicleCategory>> GetAllAsync(string? search)
        => await Query(search)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<IEnumerable<VehicleCategory>> GetPagedAsync(string? search, int pageNumber, int pageSize)
        => await Query(search)
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> CountAsync(string? search)
        => await Query(search).CountAsync();

    public async Task<VehicleCategory?> GetByIdAsync(int id)
        => await _context.VehicleCategory
            .AsNoTracking()
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.IdVehicleCategory == id);

    public async Task<VehicleCategory?> GetForUpdateAsync(int id)
        => await _context.VehicleCategory
            .FirstOrDefaultAsync(c => c.IdVehicleCategory == id);

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        => await _context.VehicleCategory
            .AnyAsync(c => c.Name == name
                && (excludeId == null || c.IdVehicleCategory != excludeId));

    public async Task<bool> HasVehiclesAsync(int id)
        => await _context.Vehicle.AnyAsync(v => v.IdVehicleCategory == id);

    public async Task AddAsync(VehicleCategory category)
    {
        _context.VehicleCategory.Add(category);
        await _context.SaveChangesAsync();
    }

    public void Remove(VehicleCategory category)
        => _context.VehicleCategory.Remove(category);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
