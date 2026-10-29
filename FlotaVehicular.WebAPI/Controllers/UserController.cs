using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de gestión de usuarios.
[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    // Lista usuarios con paginación y filtros opcionales.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] UserQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de un usuario.
    [HttpGet("{id:int}", Name = "GetUserById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // Registra un usuario nuevo.
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] UserCreateDto user)
    {
        var result = await _service.AddAsync(user);

        return CreatedAtRoute(
            "GetUserById", new { id = result.Data!.IdUser }, result);
    }

    // Actualiza nombre, usuario y correo sin tocar la contraseña.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] int id, [FromBody] UserUpdateDto user)
    {
        var result = await _service.UpdateAsync(id, user);
        return StatusCode(result.Status, result);
    }

    // Restablece la contraseña de un usuario.
    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> ChangePasswordAsync(
        [FromRoute] int id, [FromBody] UserPasswordUpdateDto user)
    {
        var result = await _service.ChangePasswordAsync(id, user);
        return StatusCode(result.Status, result);
    }

    // Asigna o modifica el rol de un usuario.
    [HttpPut("{id:int}/role")]
    public async Task<IActionResult> ChangeRoleAsync(
        [FromRoute] int id, [FromBody] UserRoleUpdateDto user)
    {
        var result = await _service.ChangeRoleAsync(id, user);
        return StatusCode(result.Status, result);
    }

    // Desactiva un usuario con baja lógica.
    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(int id)
    {
        var result = await _service.DeactivateAsync(id);
        return StatusCode(result.Status, result);
    }

    // Reactiva un usuario desactivado.
    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> ActivateAsync(int id)
    {
        var result = await _service.ActivateAsync(id);
        return StatusCode(result.Status, result);
    }
}
