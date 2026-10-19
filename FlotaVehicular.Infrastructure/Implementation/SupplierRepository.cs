using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _context;

    public SupplierRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<Supplier> ApplyFilters(IQueryable<Supplier> query, SupplierQueryDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(s => s.Name.Contains(term) || s.Contact.Contains(term));
        }

        // La desactivación no elimina las asociaciones existentes.
        if (!filters.IncludeInactive)
            query = query.Where(s => s.Status);

        return query;
    }

    private IQueryable<Supplier> BaseQuery()
        => _context.Supplier
            .AsNoTracking()
            .Include(s => s.SparePartSuppliers)
                .ThenInclude(ps => ps.SparePart);

    public async Task<IEnumerable<Supplier>> GetAllAsync(SupplierQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<IEnumerable<Supplier>> GetPagedAsync(SupplierQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(s => s.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<int> CountAsync(SupplierQueryDto query)
        => await ApplyFilters(_context.Supplier.AsNoTracking(), query).CountAsync();

    public async Task<Supplier?> GetByIdAsync(int id)
        => await _context.Supplier
            .AsNoTracking()
            .Include(s => s.SparePartSuppliers)
                .ThenInclude(ps => ps.SparePart)
            .FirstOrDefaultAsync(s => s.IdSupplier == id);

    public async Task<Supplier?> GetForUpdateAsync(int id)
        => await _context.Supplier
            .FirstOrDefaultAsync(s => s.IdSupplier == id);

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        => await _context.Supplier
            .AnyAsync(s => s.Name == name
                && (excludeId == null || s.IdSupplier != excludeId));

    public async Task<bool> HasSparePartsAsync(int id)
        => await _context.SparePartSupplier.AnyAsync(ps => ps.IdSupplier == id);

    public async Task AddAsync(Supplier supplier)
    {
        _context.Supplier.Add(supplier);
        await _context.SaveChangesAsync();
    }

    public void Remove(Supplier supplier)
        => _context.Supplier.Remove(supplier);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
