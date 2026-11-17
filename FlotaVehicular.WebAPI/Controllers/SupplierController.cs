using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.WebAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de proveedores de repuestos.
[Authorize(Roles = AppRoles.AdminOrCoordinator)]
[Route("api/supplier")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _service;

    public SupplierController(ISupplierService service)
    {
        _service = service;
    }

    // Lista los proveedores que cumplen el filtro indicado.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] SupplierQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista proveedores con paginación y búsqueda.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] SupplierQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de un proveedor con sus repuestos.
    [HttpGet("{id:int}", Name = "GetSupplierById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    // Registra un proveedor nuevo.
    [Authorize(Roles = AppRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] SupplierCreateDto supplier)
    {
        var result = await _service.AddAsync(supplier);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetSupplierById", new { id = result.Data!.IdSupplier }, result);
    }

    // Actualiza un proveedor existente.
    [Authorize(Roles = AppRoles.Administrator)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] SupplierUpdateDto supplier)
    {
        var result = await _service.UpdateAsync(id, supplier);
        return StatusCode(result.Status, result);
    }

    // Desactiva un proveedor conservando sus repuestos.
    [Authorize(Roles = AppRoles.Administrator)]
    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(int id)
    {
        var result = await _service.DeactivateAsync(id);
        return StatusCode(result.Status, result);
    }

    // Reactiva un proveedor desactivado.
    [Authorize(Roles = AppRoles.Administrator)]
    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> ActivateAsync(int id)
    {
        var result = await _service.ActivateAsync(id);
        return StatusCode(result.Status, result);
    }

    // Elimina un proveedor que no tiene repuestos asociados.
    [Authorize(Roles = AppRoles.Administrator)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
