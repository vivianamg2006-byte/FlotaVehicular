using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de las asociaciones entre técnicos y especialidades.
[Route("api/technicianspecialty")]
[ApiController]
public class TechnicianSpecialtyController : ControllerBase
{
    private readonly ITechnicianSpecialtyService _service;

    public TechnicianSpecialtyController(ITechnicianSpecialtyService service)
    {
        _service = service;
    }

    // Lista las asociaciones que cumplen el filtro indicado.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] TechnicianSpecialtyQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista asociaciones con paginación y filtros.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] TechnicianSpecialtyQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de una asociación.
    [HttpGet("{idTechnician:int}/{idSpecialty:int}", Name = "GetTechnicianSpecialtyById")]
    public async Task<IActionResult> GetByIdAsync(int idTechnician, int idSpecialty)
    {
        var result = await _service.GetByIdAsync(idTechnician, idSpecialty);
        return StatusCode(result.Status, result);
    }

    // Asocia un técnico con una especialidad.
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] TechnicianSpecialtyCreateDto link)
    {
        var result = await _service.AddAsync(link);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetTechnicianSpecialtyById",
            new { idTechnician = result.Data!.IdTechnician, idSpecialty = result.Data.IdSpecialty }, result);
    }

    // Actualiza la certificación de la asociación.
    [HttpPut("{idTechnician:int}/{idSpecialty:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int idTechnician, [FromRoute] int idSpecialty,
        [FromBody] TechnicianSpecialtyUpdateDto link)
    {
        var result = await _service.UpdateAsync(idTechnician, idSpecialty, link);
        return StatusCode(result.Status, result);
    }

    // Desasocia una especialidad del técnico.
    [HttpDelete("{idTechnician:int}/{idSpecialty:int}")]
    public async Task<IActionResult> DeleteAsync(int idTechnician, int idSpecialty)
    {
        var result = await _service.DeleteAsync(idTechnician, idSpecialty);
        return StatusCode(result.Status, result);
    }
}
