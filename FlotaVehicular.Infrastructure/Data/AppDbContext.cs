using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Data;

// Contexto de Entity Framework Core que representa la base de datos de la flota.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Catálogos y unidades de la operación.
    public DbSet<VehicleCategory> VehicleCategory { get; set; }
    public DbSet<Vehicle> Vehicle { get; set; }
    public DbSet<VehicleImage> VehicleImage { get; set; }

    // Repuestos, proveedores y sus ofertas.
    public DbSet<SparePart> SparePart { get; set; }
    public DbSet<Supplier> Supplier { get; set; }
    public DbSet<SparePartSupplier> SparePartSupplier { get; set; }

    // Especialidades, técnicos y órdenes de mantenimiento.
    public DbSet<Specialty> Specialty { get; set; }
    public DbSet<Technician> Technician { get; set; }
    public DbSet<TechnicianSpecialty> TechnicianSpecialty { get; set; }
    public DbSet<MaintenanceOrder> MaintenanceOrder { get; set; }

    // Usuarios internos del sistema.
    public DbSet<User> User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cargar las configuraciones EF de las entidades.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
