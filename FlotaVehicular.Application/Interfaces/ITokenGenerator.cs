using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Application.Interfaces;

// AccessToken generado para un usuario autenticado.
public sealed record AccessToken(string Value, DateTime ExpiresAtUtc);

// Emite tokens de acceso a partir de un usuario del sistema.
public interface ITokenGenerator
{
    AccessToken Generate(User user);
}
