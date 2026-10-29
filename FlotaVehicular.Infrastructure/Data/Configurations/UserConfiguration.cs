using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlotaVehicular.Infrastructure.Data.Configurations;

// Configuración de la tabla de usuarios del sistema.
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("User");

        entity.HasKey(e => e.IdUser);

        // El nombre de usuario y el correo identifican de forma única a cada usuario.
        entity.HasIndex(e => e.Username)
            .IsUnique();

        entity.HasIndex(e => e.Email)
            .IsUnique();

        // Índices de apoyo para los filtros y la regla del último administrador.
        entity.HasIndex(e => e.Role);
        entity.HasIndex(e => e.IsActive);

        entity.Property(e => e.FullName)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Username)
            .HasMaxLength(50)
            .IsRequired();

        entity.Property(e => e.Email)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        // El rol se guarda como texto para mantener legibilidad en SQL.
        entity.Property(e => e.Role)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(e => e.IsActive)
            .HasSentinel(true)
            .HasDefaultValue(true);

        entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(SYSUTCDATETIME())");

        entity.Property(e => e.LastAccessAt);
    }
}
