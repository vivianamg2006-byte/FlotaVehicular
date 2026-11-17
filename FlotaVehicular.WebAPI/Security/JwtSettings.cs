using System.Text;

namespace FlotaVehicular.WebAPI.Security;

// Configuración del emisor de tokens leída de la sección "Jwt".
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "";
    public string Audience { get; init; } = "";
    public string SecretKey { get; init; } = "";
    public int ExpirationMinutes { get; init; } = 60;

    // Longitud mínima de la clave en bytes para la firma HMAC-SHA256.
    public static bool HasValidKeyLength(string? secretKey)
        => !string.IsNullOrEmpty(secretKey) && Encoding.UTF8.GetByteCount(secretKey) >= 32;
}
