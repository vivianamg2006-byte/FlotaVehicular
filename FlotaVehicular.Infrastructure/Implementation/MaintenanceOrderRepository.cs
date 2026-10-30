using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

// Acceso a las órdenes de mantenimiento y sus datos de apoyo.
public class MaintenanceOrderRepository : IMaintenanceOrderRepository
{
    private readonly AppDbContext _context;

    public MaintenanceOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<MaintenanceOrder> ApplyFilters(IQueryable<MaintenanceOrder> query, MaintenanceOrderQueryDto filters)
    {
        if (filters.Status.HasValue)
            query = query.Where(o => o.Status == filters.Status.Value);

        if (filters.IdVehicle.HasValue)
            query = query.Where(o => o.IdVehicle == filters.IdVehicle.Value);

        if (filters.MaintenanceType.HasValue)
            query = query.Where(o => o.MaintenanceType == filters.MaintenanceType.Value);

        if (filters.Priority.HasValue)
            query = query.Where(o => o.Priority == filters.Priority.Value);

        return query;
    }

    private IQueryable<MaintenanceOrder> BaseQuery()
        => _context.MaintenanceOrder
            .AsNoTracking()
            .Include(o => o.Vehicle)
            .Include(o => o.RequiredSpecialty);

    public async Task<IEnumerable<MaintenanceOrder>> GetPagedAsync(MaintenanceOrderQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderByDescending(o => o.RequestDate)
            .ThenByDescending(o => o.IdMaintenanceOrder)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<IEnumerable<MaintenanceOrder>> GetAllAsync(MaintenanceOrderQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderByDescending(o => o.RequestDate)
            .ThenByDescending(o => o.IdMaintenanceOrder)
            .ToListAsync();

    public async Task<int> CountAsync(MaintenanceOrderQueryDto query)
        => await ApplyFilters(_context.MaintenanceOrder.AsNoTracking(), query).CountAsync();

    public async Task<MaintenanceOrder?> GetByIdAsync(int id)
        => await BaseQuery().FirstOrDefaultAsync(o => o.IdMaintenanceOrder == id);

    public async Task<Vehicle?> GetVehicleAsync(int idVehicle)
        => await _context.Vehicle.AsNoTracking()
            .FirstOrDefaultAsync(v => v.IdVehicle == idVehicle);

    public async Task<Specialty?> GetSpecialtyAsync(int idSpecialty)
        => await _context.Specialty.AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdSpecialty == idSpecialty);

    public async Task<int> CountByDateAsync(DateTime date)
    {
        var day = date.Date;
        var next = day.AddDays(1);

        return await _context.MaintenanceOrder
            .CountAsync(o => o.RequestDate >= day && o.RequestDate < next);
    }

    public async Task<bool> ExistsOrderCodeAsync(string orderCode)
        => await _context.MaintenanceOrder.AnyAsync(o => o.OrderCode == orderCode);

    public async Task AddAsync(MaintenanceOrder order)
    {
        _context.MaintenanceOrder.Add(order);
        await _context.SaveChangesAsync();
    }

    // La condición de estado evita sobrescribir una transición de otra solicitud.
    // Estado y fecha de cierre se guardan juntos en una única operación SQL.
    public async Task<bool> TryChangeStatusAsync(
        int id, OrderStatus expectedStatus, OrderStatus newStatus, DateTime? closedDate)
        => await _context.MaintenanceOrder
            .Where(o => o.IdMaintenanceOrder == id && o.Status == expectedStatus)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(o => o.Status, newStatus)
                .SetProperty(o => o.ClosedDate, closedDate)) == 1;

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
