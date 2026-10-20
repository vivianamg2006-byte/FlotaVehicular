using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

[Route("api/technician")]
[ApiController]
public class TechnicianController : ControllerBase
{
    private readonly ITechnicianService _service;

    public TechnicianController(ITechnicianService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] TechnicianQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] TechnicianQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("{id:int}", Name = "GetTechnicianById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] TechnicianCreateDto technician)
    {
        var result = await _service.AddAsync(technician);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetTechnicianById", new { id = result.Data!.IdTechnician }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] TechnicianUpdateDto technician)
    {
        var result = await _service.UpdateAsync(id, technician);
        return StatusCode(result.Status, result);
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(int id)
    {
        var result = await _service.DeactivateAsync(id);
        return StatusCode(result.Status, result);
    }

    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> ActivateAsync(int id)
    {
        var result = await _service.ActivateAsync(id);
        return StatusCode(result.Status, result);
    }
}
