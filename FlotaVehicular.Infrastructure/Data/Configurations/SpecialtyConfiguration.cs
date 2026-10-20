using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> entity)
    {
        entity.ToTable("Specialty");

        entity.HasKey(e => e.IdSpecialty);

        // El nombre evita duplicar la misma especialidad.
        entity.HasIndex(e => e.Name)
            .IsUnique();

        entity.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Description)
            .HasMaxLength(500);
    }
}
