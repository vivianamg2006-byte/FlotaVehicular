using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

// Acceso a los vehículos, sus categorías e historial de órdenes.
public class VehicleRepository : IVehicleRepository
{
    private readonly AppDbContext _context;

    public VehicleRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<Vehicle> ApplyFilters(IQueryable<Vehicle> query, VehicleQueryDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(v =>
                v.Code.Contains(term) ||
                v.LicensePlate.Contains(term) ||
                v.Name.Contains(term) ||
                v.Brand.Contains(term) ||
                v.Model.Contains(term));
        }

        if (filters.IdVehicleCategory.HasValue)
            query = query.Where(v => v.IdVehicleCategory == filters.IdVehicleCategory.Value);

        if (filters.Status.HasValue)
            query = query.Where(v => v.Status == filters.Status.Value);

        // El listado normal no muestra vehículos dados de baja.
        if (!filters.IncludeDeactivated)
            query = query.Where(v => v.Status != VehicleStatus.DadoDeBaja);

        return query;
    }

    // Las consultas incluyen los datos que necesitan los DTO.
    private IQueryable<Vehicle> BaseQuery()
        => _context.Vehicle
            .AsNoTracking()
            .Include(v => v.Category)
            .Include(v => v.Images);

    public async Task<IEnumerable<Vehicle>> GetAllAsync(VehicleQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(v => v.Code)
            .ToListAsync();

    public async Task<IEnumerable<Vehicle>> GetPagedAsync(VehicleQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(v => v.Code)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<int> CountAsync(VehicleQueryDto query)
        => await ApplyFilters(_context.Vehicle.AsNoTracking(), query).CountAsync();

    public async Task<Vehicle?> GetByIdAsync(int id)

        => await _context.Vehicle
            .AsNoTrackingWithIdentityResolution()
            .Include(v => v.Category)
                .ThenInclude(c => c.Vehicles)
            .Include(v => v.Images)
            .FirstOrDefaultAsync(v => v.IdVehicle == id);

    public async Task<Vehicle?> GetForUpdateAsync(int id)
        => await _context.Vehicle
            .FirstOrDefaultAsync(v => v.IdVehicle == id);

    public async Task<Vehicle?> GetForDeactivationAsync(int id)
        => await _context.Vehicle
            .FirstOrDefaultAsync(v => v.IdVehicle == id && v.Status != VehicleStatus.DadoDeBaja);

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        => await _context.Vehicle
            .AnyAsync(v => v.Code == code
                && (excludeId == null || v.IdVehicle != excludeId));

    public async Task<bool> ExistsByLicensePlateAsync(string licensePlate, int? excludeId = null)
        => await _context.Vehicle
            .AnyAsync(v => v.LicensePlate == licensePlate
                && (excludeId == null || v.IdVehicle != excludeId));

    public async Task<bool> CategoryExistsAsync(int idVehicleCategory)
        => await _context.VehicleCategory.AnyAsync(c => c.IdVehicleCategory == idVehicleCategory);

    public async Task AddAsync(Vehicle vehicle)
    {
        _context.Vehicle.Add(vehicle);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
