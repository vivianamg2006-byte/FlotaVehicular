using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

public class VehicleCategoryConfiguration : IEntityTypeConfiguration<VehicleCategory>
{
    public void Configure(EntityTypeBuilder<VehicleCategory> entity)
    {
        entity.ToTable("VehicleCategory");

        entity.HasKey(e => e.IdVehicleCategory);

        // La base de datos también impide nombres repetidos.
        entity.HasIndex(e => e.Name)
            .IsUnique();

        entity.Property(e => e.Name)
            .HasMaxLength(80)
            .IsRequired();

        entity.Property(e => e.Description)
            .HasMaxLength(500);

        entity.Property(e => e.PreventiveMaintenanceIntervalKm)
            .IsRequired();

        entity.HasMany(e => e.Vehicles)
            .WithOne(v => v.Category)
            .HasForeignKey(v => v.IdVehicleCategory)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
