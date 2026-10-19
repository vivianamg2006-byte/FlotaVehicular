using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Implementation;

public class VehicleImageRepository : IVehicleImageRepository
{
    private readonly AppDbContext _context;

    public VehicleImageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VehicleImage>> GetByVehicleAsync(int vehicleId)
        => await _context.VehicleImage
            .AsNoTracking()
            .Where(i => i.IdVehicle == vehicleId)
            .OrderByDescending(i => i.IsMain)
            .ThenByDescending(i => i.UploadedAt)
            .ToListAsync();

    public async Task<IEnumerable<VehicleImage>> GetTrackedByVehicleAsync(int vehicleId)
        => await _context.VehicleImage
            .Where(i => i.IdVehicle == vehicleId)
            .OrderByDescending(i => i.IsMain)
            .ThenByDescending(i => i.UploadedAt)
            .ToListAsync();

    public async Task<VehicleImage?> GetByIdAsync(int vehicleId, int imageId)
        => await _context.VehicleImage
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.IdVehicle == vehicleId && i.IdVehicleImage == imageId);

    public async Task<Vehicle?> GetVehicleForUpdateAsync(int vehicleId)
        => await _context.Vehicle
            .FirstOrDefaultAsync(v => v.IdVehicle == vehicleId);

    public async Task AddAsync(VehicleImage image)
    {
        _context.VehicleImage.Add(image);
        await _context.SaveChangesAsync();
    }

    public void Remove(VehicleImage image)
        => _context.VehicleImage.Remove(image);

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
