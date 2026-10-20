using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<VehicleCategory> VehicleCategory { get; set; }
    public DbSet<Vehicle> Vehicle { get; set; }
    public DbSet<VehicleImage> VehicleImage { get; set; }
    public DbSet<SparePart> SparePart { get; set; }
    public DbSet<Supplier> Supplier { get; set; }
    public DbSet<SparePartSupplier> SparePartSupplier { get; set; }
    public DbSet<Specialty> Specialty { get; set; }
    public DbSet<Technician> Technician { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cargar las configuraciones EF de las entidades.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
