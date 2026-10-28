using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints del catálogo de repuestos y su nivel de stock.
[Route("api/sparepart")]
[ApiController]
public class SparePartController : ControllerBase
{
    private readonly ISparePartService _service;

    public SparePartController(ISparePartService service)
    {
        _service = service;
    }

    // Lista los repuestos que cumplen el filtro indicado.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] SparePartQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista repuestos con paginación y filtros.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] SparePartQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista repuestos cuyo stock está por debajo del mínimo.
    [HttpGet("below-minimum-stock")]
    public async Task<IActionResult> GetBelowMinimumStockAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetBelowMinimumStockAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de un repuesto con sus proveedores.
    [HttpGet("{id:int}", Name = "GetSparePartById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // Registra un repuesto nuevo.
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] SparePartCreateDto sparePart)
    {
        var result = await _service.AddAsync(sparePart);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetSparePartById", new { id = result.Data!.IdSparePart }, result);
    }

    // Actualiza un repuesto existente.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] SparePartUpdateDto sparePart)
    {
        var result = await _service.UpdateAsync(id, sparePart);
        return StatusCode(result.Status, result);
    }

    // Elimina un repuesto que no tiene proveedores asociados.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
