using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

[Route("api/technicianspecialty")]
[ApiController]
public class TechnicianSpecialtyController : ControllerBase
{
    private readonly ITechnicianSpecialtyService _service;

    public TechnicianSpecialtyController(ITechnicianSpecialtyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] TechnicianSpecialtyQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] TechnicianSpecialtyQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("{idTechnician:int}/{idSpecialty:int}", Name = "GetTechnicianSpecialtyById")]
    public async Task<IActionResult> GetByIdAsync(int idTechnician, int idSpecialty)
    {
        var result = await _service.GetByIdAsync(idTechnician, idSpecialty);
        return StatusCode(result.Status, result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] TechnicianSpecialtyCreateDto link)
    {
        var result = await _service.AddAsync(link);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetTechnicianSpecialtyById",
            new { idTechnician = result.Data!.IdTechnician, idSpecialty = result.Data.IdSpecialty }, result);
    }

    [HttpPut("{idTechnician:int}/{idSpecialty:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int idTechnician, [FromRoute] int idSpecialty,
        [FromBody] TechnicianSpecialtyUpdateDto link)
    {
        var result = await _service.UpdateAsync(idTechnician, idSpecialty, link);
        return StatusCode(result.Status, result);
    }

    [HttpDelete("{idTechnician:int}/{idSpecialty:int}")]
    public async Task<IActionResult> DeleteAsync(int idTechnician, int idSpecialty)
    {
        var result = await _service.DeleteAsync(idTechnician, idSpecialty);
        return StatusCode(result.Status, result);
    }
}
