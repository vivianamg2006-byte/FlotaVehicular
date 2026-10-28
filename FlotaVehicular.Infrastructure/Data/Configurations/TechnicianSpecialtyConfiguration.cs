using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

// Configuración de la tabla de asociaciones técnico-especialidad.
public class TechnicianSpecialtyConfiguration : IEntityTypeConfiguration<TechnicianSpecialty>
{
    public void Configure(EntityTypeBuilder<TechnicianSpecialty> entity)
    {
        entity.ToTable("TechnicianSpecialty");

        // La clave compuesta impide repetir la misma pareja técnico-especialidad.
        entity.HasKey(e => new { e.IdTechnician, e.IdSpecialty });

        entity.HasIndex(e => e.IdSpecialty);

        entity.Property(e => e.CertificationLevel)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.SinceDate)
            .HasColumnType("date");

        entity.HasOne(e => e.Technician)
            .WithMany(t => t.TechnicianSpecialties)
            .HasForeignKey(e => e.IdTechnician)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Specialty)
            .WithMany(s => s.TechnicianSpecialties)
            .HasForeignKey(e => e.IdSpecialty)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
