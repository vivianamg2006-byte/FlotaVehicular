using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de categorías de vehículos y sus vehículos asociados.
[Route("api/vehiclecategory")]
[ApiController]
public class VehicleCategoryController : ControllerBase
{
    private readonly IVehicleCategoryService _service;

    public VehicleCategoryController(IVehicleCategoryService service)
    {
        _service = service;
    }

    // Lista las categorías que cumplen el criterio de búsqueda.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] string? search)
    {
        var result = await _service.GetAllAsync(search);
        return StatusCode(result.Status, result);
    }

    // Lista categorías con paginación y búsqueda.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] VehicleCategoryQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de una categoría con sus vehículos.
    [HttpGet("{id:int}", Name = "GetVehicleCategoryById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // Registra una categoría nueva.
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] VehicleCategoryCreateDto category)
    {
        var result = await _service.AddAsync(category);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetVehicleCategoryById", new { id = result.Data!.IdVehicleCategory }, result);
    }

    // Actualiza una categoría existente.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] VehicleCategoryUpdateDto category)
    {
        var result = await _service.UpdateAsync(id, category);
        return StatusCode(result.Status, result);
    }

    // Elimina una categoría que no tiene vehículos asociados.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
