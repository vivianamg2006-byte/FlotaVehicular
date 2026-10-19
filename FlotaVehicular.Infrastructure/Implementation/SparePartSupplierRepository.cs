using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

public class SparePartSupplierRepository : ISparePartSupplierRepository
{
    private readonly AppDbContext _context;

    public SparePartSupplierRepository(AppDbContext context)
    {
        _context = context;
    }

    private static IQueryable<SparePartSupplier> ApplyFilters(IQueryable<SparePartSupplier> query, SparePartSupplierQueryDto filters)
    {
        if (filters.IdSparePart.HasValue)
            query = query.Where(ps => ps.IdSparePart == filters.IdSparePart.Value);

        if (filters.IdSupplier.HasValue)
            query = query.Where(ps => ps.IdSupplier == filters.IdSupplier.Value);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.Trim();
            query = query.Where(ps => ps.SparePart.Code.Contains(term)
                || ps.SparePart.Name.Contains(term)
                || ps.Supplier.Name.Contains(term));
        }

        return query;
    }

    private IQueryable<SparePartSupplier> BaseQuery()
        => _context.SparePartSupplier
            .AsNoTracking()
            .Include(ps => ps.SparePart)
            .Include(ps => ps.Supplier);

    public async Task<IEnumerable<SparePartSupplier>> GetAllAsync(SparePartSupplierQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(ps => ps.SparePart.Code)
            .ThenBy(ps => ps.Supplier.Name)
            .ToListAsync();

    public async Task<IEnumerable<SparePartSupplier>> GetPagedAsync(SparePartSupplierQueryDto query)
        => await ApplyFilters(BaseQuery(), query)
            .OrderBy(ps => ps.SparePart.Code)
            .ThenBy(ps => ps.Supplier.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

    public async Task<int> CountAsync(SparePartSupplierQueryDto query)
        => await ApplyFilters(_context.SparePartSupplier.AsNoTracking(), query).CountAsync();

    public async Task<SparePartSupplier?> GetByIdAsync(int idSparePart, int idSupplier)
        => await _context.SparePartSupplier
            .AsNoTracking()
            .Include(ps => ps.SparePart)
            .Include(ps => ps.Supplier)
            .FirstOrDefaultAsync(ps => ps.IdSparePart == idSparePart && ps.IdSupplier == idSupplier);

    public async Task<SparePartSupplier?> GetForUpdateAsync(int idSparePart, int idSupplier)
        => await _context.SparePartSupplier
            .FirstOrDefaultAsync(ps => ps.IdSparePart == idSparePart && ps.IdSupplier == idSupplier);

    public async Task<bool> ExistsAsync(int idSparePart, int idSupplier)
        => await _context.SparePartSupplier
            .AnyAsync(ps => ps.IdSparePart == idSparePart && ps.IdSupplier == idSupplier);

    public async Task<bool> SparePartExistsAsync(int idSparePart)
        => await _context.SparePart.AnyAsync(s => s.IdSparePart == idSparePart);

    public async Task<Supplier?> GetSupplierAsync(int idSupplier)
        => await _context.Supplier.AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdSupplier == idSupplier);

    public async Task AddAsync(SparePartSupplier link)
    {
        _context.SparePartSupplier.Add(link);
        await _context.SaveChangesAsync();
    }

    public void Remove(SparePartSupplier link)
        => _context.SparePartSupplier.Remove(link);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
