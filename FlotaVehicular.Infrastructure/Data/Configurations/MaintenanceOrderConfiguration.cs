using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

public class MaintenanceOrderConfiguration : IEntityTypeConfiguration<MaintenanceOrder>
{
    public void Configure(EntityTypeBuilder<MaintenanceOrder> entity)
    {
        entity.ToTable("MaintenanceOrder");

        entity.HasKey(e => e.IdMaintenanceOrder);

        // El código OM-yyyyMMdd-0001 identifica a la orden ante el usuario.
        entity.HasIndex(e => e.OrderCode)
            .IsUnique();

        entity.HasIndex(e => e.IdVehicle);
        entity.HasIndex(e => e.Status);

        entity.Property(e => e.OrderCode)
            .HasMaxLength(20)
            .IsRequired();

        entity.Property(e => e.MaintenanceType)
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.Property(e => e.Reason)
            .HasMaxLength(500)
            .IsRequired();

        entity.Property(e => e.RequestDate)
            .HasDefaultValueSql("(SYSUTCDATETIME())");

        entity.Property(e => e.ScheduledDate);
        entity.Property(e => e.ClosedDate);

        entity.Property(e => e.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.Property(e => e.TotalCost)
            .HasColumnType("decimal(18, 2)")
            .HasDefaultValue(0m);

        entity.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(SYSUTCDATETIME())");

        // Ninguna orden se elimina en cascada: el historial debe conservarse.
        entity.HasOne(e => e.Vehicle)
            .WithMany()
            .HasForeignKey(e => e.IdVehicle)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.RequiredSpecialty)
            .WithMany(s => s.MaintenanceOrders)
            .HasForeignKey(e => e.IdRequiredSpecialty)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
