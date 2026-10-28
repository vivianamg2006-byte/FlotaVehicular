using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

// Configuración de la tabla de imágenes de vehículos.
public class VehicleImageConfiguration : IEntityTypeConfiguration<VehicleImage>
{
    public void Configure(EntityTypeBuilder<VehicleImage> entity)
    {
        entity.ToTable("VehicleImage");

        entity.HasKey(e => e.IdVehicleImage);

        entity.HasIndex(e => new { e.IdVehicle, e.FileName })
            .IsUnique();

        entity.HasIndex(e => e.IdVehicle);

        entity.Property(e => e.FileName)
            .HasMaxLength(150)
            .IsRequired();

        entity.Property(e => e.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        entity.Property(e => e.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.RelativePath)
            .HasMaxLength(400)
            .IsRequired();

        entity.Property(e => e.PublicUrl)
            .HasMaxLength(400)
            .IsRequired();

        entity.Property(e => e.UploadedAt)
            .HasDefaultValueSql("(SYSUTCDATETIME())");

        entity.HasOne(e => e.Vehicle)
            .WithMany(v => v.Images)
            .HasForeignKey(e => e.IdVehicle)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
