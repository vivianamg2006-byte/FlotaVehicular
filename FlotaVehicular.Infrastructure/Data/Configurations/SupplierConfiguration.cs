using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> entity)
    {
        entity.ToTable("Supplier");

        entity.HasKey(e => e.IdSupplier);

        entity.HasIndex(e => e.Name)
            .IsUnique();

        entity.HasIndex(e => e.Status);

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Contact)
            .HasMaxLength(150)
            .IsRequired();

        entity.Property(e => e.Address)
            .HasMaxLength(250);

        entity.Property(e => e.GeneralConditions)
            .HasMaxLength(2000);

        entity.Property(e => e.Status)
            .HasSentinel(true);
    }
}
