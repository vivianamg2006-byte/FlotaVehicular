using System.Globalization;
using System.Security.Claims;
using System.Text;
using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FlotaVehicular.WebAPI.Security;

// Genera JWT firmados con HMAC-SHA256 usando la configuración de la sección "Jwt".
public sealed class JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtSettings _settings;
    private readonly JsonWebTokenHandler _handler = new();

    public JwtTokenGenerator(JwtSettings settings)
    {
        _settings = settings;
    }

    // Incluye identidad, nombre de usuario, rol y nombre completo como claims.
    public AccessToken Generate(User user)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_settings.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("fullName", user.FullName)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }
}
