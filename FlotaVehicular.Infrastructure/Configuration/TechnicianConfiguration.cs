using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Configuration;

public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.ToTable("Technician");

        builder.HasKey(t => t.IdTechnician);

        builder.Property(t => t.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.IdentificationNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(t => t.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Phone)
            .HasMaxLength(20);

        builder.Property(t => t.HourlyRate)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("(SYSUTCDATETIME())");

        builder.HasIndex(t => t.IdentificationNumber)
            .IsUnique();

        builder.HasIndex(t => t.Email)
            .IsUnique();

        builder.HasIndex(t => t.IsActive);

        builder.HasIndex(t => t.IsAvailable);
    }
}
