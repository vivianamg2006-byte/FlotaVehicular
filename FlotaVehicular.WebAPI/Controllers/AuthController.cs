using System.Security.Claims;
using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de autenticación: inicio de sesión, cierre y usuario actual.
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private const string InvalidSessionMessage = "La sesión ya no es válida. Inicie sesión nuevamente.";

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // Inicia sesión con nombre de usuario o correo y contraseña.
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return StatusCode(result.Status, result);
    }

    // El token no tiene estado: el cliente es el único que debe descartarlo.
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
        => StatusCode(200, Result<object>.Success(new { }, 200, "OK", "Sesión cerrada."));

    // Devuelve el usuario asociado al token vigente.
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var idUser) || idUser <= 0)
            return StatusCode(401, Result<AuthUserDto>.Failure(401, "Unauthorized", InvalidSessionMessage));

        var result = await _authService.GetCurrentUserAsync(idUser);
        return StatusCode(result.Status, result);
    }
}
