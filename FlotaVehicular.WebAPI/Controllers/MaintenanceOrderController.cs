using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de órdenes de mantenimiento.
[Route("api/maintenanceorder")]
[ApiController]
public class MaintenanceOrderController : ControllerBase
{
    private readonly IMaintenanceOrderService _service;

    public MaintenanceOrderController(IMaintenanceOrderService service)
    {
        _service = service;
    }

    // Lista las órdenes que cumplen el filtro indicado.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] MaintenanceOrderQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista órdenes con paginación y filtros.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] MaintenanceOrderQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de una orden de mantenimiento.
    [HttpGet("{id:int}", Name = "GetMaintenanceOrderById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // Crea una orden de mantenimiento.
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] MaintenanceOrderCreateDto order)
    {
        var result = await _service.AddAsync(order);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetMaintenanceOrderById", new { id = result.Data!.IdMaintenanceOrder }, result);
    }
}
