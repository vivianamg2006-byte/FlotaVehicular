using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de especialidades de mantenimiento.
[Route("api/specialty")]
[ApiController]
public class SpecialtyController : ControllerBase
{
    private readonly ISpecialtyService _service;

    public SpecialtyController(ISpecialtyService service)
    {
        _service = service;
    }

    // Lista las especialidades que cumplen el filtro indicado.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] SpecialtyQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista especialidades con paginación y búsqueda.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] SpecialtyQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de una especialidad con sus técnicos.
    [HttpGet("{id:int}", Name = "GetSpecialtyById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // Registra una especialidad nueva.
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] SpecialtyCreateDto specialty)
    {
        var result = await _service.AddAsync(specialty);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetSpecialtyById", new { id = result.Data!.IdSpecialty }, result);
    }

    // Actualiza una especialidad existente.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] SpecialtyUpdateDto specialty)
    {
        var result = await _service.UpdateAsync(id, specialty);
        return StatusCode(result.Status, result);
    }

    // Elimina una especialidad que no tiene técnicos asociados.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
