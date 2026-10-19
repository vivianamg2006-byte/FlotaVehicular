using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

[Route("api/vehiclecategory")]
[ApiController]
public class VehicleCategoryController : ControllerBase
{
    private readonly IVehicleCategoryService _service;

    public VehicleCategoryController(IVehicleCategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] string? search)
    {
        var result = await _service.GetAllAsync(search);
        return StatusCode(result.Status, result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] VehicleCategoryQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("{id:int}", Name = "GetVehicleCategoryById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] VehicleCategoryCreateDto category)
    {
        var result = await _service.AddAsync(category);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetVehicleCategoryById", new { id = result.Data!.IdVehicleCategory }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] VehicleCategoryUpdateDto category)
    {
        var result = await _service.UpdateAsync(id, category);
        return StatusCode(result.Status, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
