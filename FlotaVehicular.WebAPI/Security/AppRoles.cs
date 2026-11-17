using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.WebAPI.Security;

// Conjuntos de roles usados por los atributos [Authorize] de los controladores.
public static class AppRoles
{
    public const string Administrator = nameof(UserRole.Administrador);
    public const string Coordinator = nameof(UserRole.CoordinadorTecnico);
    public const string Requester = nameof(UserRole.SolicitanteConsulta);

    // Administrador o Coordinador Técnico.
    public const string AdminOrCoordinator = Administrator + "," + Coordinator;

    // Administrador o Solicitante de Consulta.
    public const string AdminOrRequester = Administrator + "," + Requester;
}
