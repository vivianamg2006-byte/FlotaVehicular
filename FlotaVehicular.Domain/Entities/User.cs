namespace FlotaVehicular.Domain.Entities;

// Usuario interno del sistema, independiente de la autenticación.
public class User
{
    // Identificador único del usuario.
    public int IdUser { get; set; }

    // Nombre completo del usuario.
    public string FullName { get; set; } = null!;

    // Nombre de usuario utilizado para iniciar sesión; es único.
    public string Username { get; set; } = null!;

    // Correo del usuario; es único y debe tener formato válido.
    public string Email { get; set; } = null!;

    // Hash de la contraseña; nunca se almacena ni se expone la contraseña original.
    public string PasswordHash { get; set; } = null!;

    // Rol asignado al usuario dentro del sistema.
    public UserRole Role { get; set; }

    // La baja es lógica para conservar la trazabilidad del usuario.
    public bool IsActive { get; set; }

    // Fecha de creación del usuario.
    public DateTime CreatedAt { get; set; }

    // Último acceso; queda disponible para cuando exista el login.
    public DateTime? LastAccessAt { get; set; }
}
