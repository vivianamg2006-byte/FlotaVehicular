using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

[Route("api/vehicle")]
[ApiController]
public class VehicleController : ControllerBase
{
    private readonly IVehicleService _service;

    public VehicleController(IVehicleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] VehicleQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] VehicleQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("{id:int}", Name = "GetVehicleById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] VehicleCreateDto vehicle)
    {
        var result = await _service.AddAsync(vehicle);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetVehicleById", new { id = result.Data!.IdVehicle }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] VehicleUpdateDto vehicle)
    {
        var result = await _service.UpdateAsync(id, vehicle);
        return StatusCode(result.Status, result);
    }

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
