using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

[Route("api/sparepart")]
[ApiController]
public class SparePartController : ControllerBase
{
    private readonly ISparePartService _service;

    public SparePartController(ISparePartService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] SparePartQueryDto query)
    {
        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] SparePartQueryDto query)
    {
        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    [HttpGet("below-minimum-stock")]
    public async Task<IActionResult> GetBelowMinimumStockAsync([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetBelowMinimumStockAsync(pageNumber, pageSize);
        return StatusCode(result.Status, result);
    }

    [HttpGet("{id:int}", Name = "GetSparePartById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return StatusCode(result.Status, result);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] SparePartCreateDto sparePart)
    {
        var result = await _service.AddAsync(sparePart);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetSparePartById", new { id = result.Data!.IdSparePart }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] SparePartUpdateDto sparePart)
    {
        var result = await _service.UpdateAsync(id, sparePart);
        return StatusCode(result.Status, result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _service.DeleteAsync(id);
        return StatusCode(result.Status, result);
    }
}
