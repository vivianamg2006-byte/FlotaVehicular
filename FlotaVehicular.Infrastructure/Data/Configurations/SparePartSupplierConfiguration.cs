using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

public class SparePartSupplierConfiguration : IEntityTypeConfiguration<SparePartSupplier>
{
    public void Configure(EntityTypeBuilder<SparePartSupplier> entity)
    {
        entity.ToTable("SparePartSupplier");

        // La clave compuesta impide repetir la misma pareja.
        entity.HasKey(e => new { e.IdSparePart, e.IdSupplier });

        entity.HasIndex(e => e.IdSupplier);

        entity.Property(e => e.OfferedPrice)
            .HasColumnType("decimal(18, 2)");

        entity.Property(e => e.EstimatedDeliveryDays).IsRequired();

        entity.Property(e => e.AssociatedAt)
            .HasDefaultValueSql("(SYSUTCDATETIME())");

        entity.HasOne(e => e.SparePart)
            .WithMany(s => s.SparePartSuppliers)
            .HasForeignKey(e => e.IdSparePart)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Supplier)
            .WithMany(s => s.SparePartSuppliers)
            .HasForeignKey(e => e.IdSupplier)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
