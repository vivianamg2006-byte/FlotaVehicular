using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

[Route("api/supplier")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _service;

    public SupplierController(ISupplierService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] SupplierQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] SupplierQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("{id:int}", Name = "GetSupplierById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] SupplierCreateDto supplier)
    {
        var result = await _service.AddAsync(supplier);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetSupplierById", new { id = result.Data!.IdSupplier }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] SupplierUpdateDto supplier)
    {
        var result = await _service.UpdateAsync(id, supplier);
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
