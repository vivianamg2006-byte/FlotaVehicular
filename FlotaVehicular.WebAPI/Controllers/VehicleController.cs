using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de vehículos: catálogo, filtros, mantenimiento preventivo y baja lógica.
[Route("api/vehicle")]
[ApiController]
public class VehicleController : ControllerBase
{
    private readonly IVehicleService _service;

    public VehicleController(IVehicleService service)
    {
        _service = service;
    }

    // Lista los vehículos que cumplen el filtro indicado.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] VehicleQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista vehículos con paginación y filtros.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] VehicleQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de un vehículo con su categoría e imágenes.
    [HttpGet("{id:int}", Name = "GetVehicleById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // Registra un vehículo nuevo; nace en estado operativo.
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] VehicleCreateDto vehicle)
    {
        var result = await _service.AddAsync(vehicle);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetVehicleById", new { id = result.Data!.IdVehicle }, result);
    }

    // Actualiza un vehículo que no está dado de baja.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] VehicleUpdateDto vehicle)
    {
        var result = await _service.UpdateAsync(id, vehicle);
        return StatusCode(result.Status, result);
    }

    // Cambia el estado operativo de un vehículo.
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatusAsync([FromRoute] int id, [FromBody] VehicleStatusDto status)
    {
        var result = await _service.ChangeStatusAsync(id, status);
        return StatusCode(result.Status, result);
    }

    // Esta operación da de baja el vehículo; no elimina la fila.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateAsync(int id)
    {
        var result = await _service.DeactivateAsync(id);
        return StatusCode(result.Status, result);
    }
}
