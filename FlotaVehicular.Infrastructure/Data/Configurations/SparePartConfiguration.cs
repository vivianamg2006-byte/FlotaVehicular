using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

public class SparePartConfiguration : IEntityTypeConfiguration<SparePart>
{
    public void Configure(EntityTypeBuilder<SparePart> entity)
    {
        // El stock tampoco puede ser negativo al guardar directamente en SQL.
        entity.ToTable("SparePart", table =>
        {
            table.HasCheckConstraint("CK_SparePart_CurrentStock", "[CurrentStock] >= 0");
            table.HasCheckConstraint("CK_SparePart_MinStock", "[MinStock] >= 0");
        });

        entity.HasKey(e => e.IdSparePart);

        entity.HasIndex(e => e.Code)
            .IsUnique();

        entity.Property(e => e.Code)
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.UnitOfMeasure)
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(e => e.CurrentStock).IsRequired();
        entity.Property(e => e.MinStock).IsRequired();
    }
}
