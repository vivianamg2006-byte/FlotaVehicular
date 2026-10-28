using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

// Configuración de la tabla de vehículos y su categoría.
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> entity)
    {
        entity.ToTable("Vehicle");

        entity.HasKey(e => e.IdVehicle);

        entity.HasIndex(e => e.Code)
            .IsUnique();

        entity.HasIndex(e => e.LicensePlate)
            .IsUnique();

        entity.HasIndex(e => e.Status);

        entity.Property(e => e.Code)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(e => e.LicensePlate)
            .HasMaxLength(15)
            .IsRequired();

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Description)
            .HasMaxLength(500);

        entity.Property(e => e.AcquisitionDate)
            .HasColumnType("date");

        entity.Property(e => e.ReferenceValue)
            .HasColumnType("decimal(18, 2)");

        entity.Property(e => e.ImagePath)
            .HasMaxLength(300);

        entity.Property(e => e.Brand)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.Model)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.Year)
            .IsRequired();

        entity.Property(e => e.CurrentMileage).IsRequired();
        entity.Property(e => e.LastPreventiveMaintenanceMileage).IsRequired();

        entity.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(SYSUTCDATETIME())");

        entity.Property(e => e.DeactivationDate);

        // Una categoría usada por vehículos no se puede eliminar.
        entity.HasOne(e => e.Category)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(e => e.IdVehicleCategory)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
