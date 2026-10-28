using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

// Configuración de la tabla de técnicos.
public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> entity)
    {
        entity.ToTable("Technician");

        entity.HasKey(e => e.IdTechnician);

        // La identificación es el documento único de cada técnico.
        entity.HasIndex(e => e.IdentificationNumber)
            .IsUnique();

        entity.HasIndex(e => e.IsAvailable);
        entity.HasIndex(e => e.IsActive);

        entity.Property(e => e.FullName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.IdentificationNumber)
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(e => e.Email)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Phone)
            .HasMaxLength(20);

        entity.Property(e => e.IsAvailable)
            .HasSentinel(true);

        entity.Property(e => e.HourlyRate)
            .HasColumnType("decimal(18, 2)");

        entity.Property(e => e.IsActive)
            .HasSentinel(true);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(SYSUTCDATETIME())");
    }
}
