using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

// Acceso al catálogo de repuestos y su nivel de stock.
public class SparePartRepository : ISparePartRepository
{
    private readonly AppDbContext _context;

    public SparePartRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<SparePart> ApplyFilters(IQueryable<SparePart> query, SparePartQueryDto filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(s => s.Code.Contains(term) || s.Name.Contains(term));
        }

        // Se compara el stock de cada repuesto con su propio mínimo.
        if (filters.BelowMinimumOnly)
            query = query.Where(s => s.CurrentStock < s.MinStock);

        return query;
    }

    private IQueryable<SparePart> BaseQuery()
        => _context.SparePart
            .AsNoTracking()
            .Include(s => s.SparePartSuppliers)
                .ThenInclude(ps => ps.Supplier);

    public async Task<IEnumerable<SparePart>> GetAllAsync(SparePartQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(s => s.Code)
            .ToListAsync();

    public async Task<IEnumerable<SparePart>> GetPagedAsync(SparePartQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(s => s.Code)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<int> CountAsync(SparePartQueryDto query)
        => await ApplyFilters(_context.SparePart.AsNoTracking(), query).CountAsync();

    public async Task<SparePart?> GetByIdAsync(int id)
        => await _context.SparePart
            .AsNoTracking()
            .Include(s => s.SparePartSuppliers)
                .ThenInclude(ps => ps.Supplier)
            .FirstOrDefaultAsync(s => s.IdSparePart == id);

    public async Task<SparePart?> GetForUpdateAsync(int id)
        => await _context.SparePart
            .FirstOrDefaultAsync(s => s.IdSparePart == id);

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        => await _context.SparePart
            .AnyAsync(s => s.Code == code
                && (excludeId == null || s.IdSparePart != excludeId));

    public async Task<bool> HasSuppliersAsync(int id)
        => await _context.SparePartSupplier.AnyAsync(ps => ps.IdSparePart == id);

    public async Task AddAsync(SparePart sparePart)
    {
        _context.SparePart.Add(sparePart);
        await _context.SaveChangesAsync();
    }

    public void Remove(SparePart sparePart)
        => _context.SparePart.Remove(sparePart);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
