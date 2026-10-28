using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de las ofertas de compra entre repuestos y proveedores.
[Route("api/sparepartsupplier")]
[ApiController]
public class SparePartSupplierController : ControllerBase
{
    private readonly ISparePartSupplierService _service;

    public SparePartSupplierController(ISparePartSupplierService service)
    {
        _service = service;
    }

    // Lista las ofertas que cumplen el filtro indicado.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] SparePartSupplierQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista ofertas con paginación y filtros.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] SparePartSupplierQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de una oferta de repuesto-proveedor.
    [HttpGet("{idSparePart:int}/{idSupplier:int}", Name = "GetSparePartSupplierById")]
    public async Task<IActionResult> GetByIdAsync(int idSparePart, int idSupplier)
    {
        var result = await _service.GetByIdAsync(idSparePart, idSupplier);
        return StatusCode(result.Status, result);
    }

    // Registra una oferta entre un repuesto y un proveedor.
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] SparePartSupplierCreateDto link)
    {
        var result = await _service.AddAsync(link);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetSparePartSupplierById",
            new { idSparePart = result.Data!.IdSparePart, idSupplier = result.Data.IdSupplier }, result);
    }

    // Actualiza las condiciones de la oferta.
    [HttpPut("{idSparePart:int}/{idSupplier:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int idSparePart, [FromRoute] int idSupplier,
        [FromBody] SparePartSupplierUpdateDto link)
    {
        var result = await _service.UpdateAsync(idSparePart, idSupplier, link);
        return StatusCode(result.Status, result);
    }

    // Elimina la oferta entre repuesto y proveedor.
    [HttpDelete("{idSparePart:int}/{idSupplier:int}")]
    public async Task<IActionResult> DeleteAsync(int idSparePart, int idSupplier)
    {
        var result = await _service.DeleteAsync(idSparePart, idSupplier);
        return StatusCode(result.Status, result);
    }
}
