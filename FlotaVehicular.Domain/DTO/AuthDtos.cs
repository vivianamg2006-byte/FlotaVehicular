using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.DTO;

// Credenciales enviadas por el cliente; admite nombre de usuario o correo.
public record class LoginRequestDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(100, ErrorMessage = "El usuario no puede superar 100 caracteres.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(128, ErrorMessage = "La contraseña no puede superar 128 caracteres.")]
    public string? Password { get; set; }
}

// Datos públicos del usuario autenticado; nunca incluye el hash de la contraseña.
public record class AuthUserDto
{
    public int IdUser { get; set; }
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}

// Respuesta de inicio de sesión con el token y sus tiempos de validez.
public record class LoginResponseDto
{
    public string Token { get; set; } = "";
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public int ExpiresInSeconds { get; set; }
    public AuthUserDto User { get; set; } = new();
}
