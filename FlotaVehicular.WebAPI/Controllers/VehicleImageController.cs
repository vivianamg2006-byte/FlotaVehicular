using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de imágenes de vehículos: carga, descarga y reemplazo.
[Route("api/vehicle/{vehicleId:int}/images")]
[ApiController]
public class VehicleImageController : ControllerBase
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IVehicleImageService _service;

    public VehicleImageController(IVehicleImageService service)
    {
        _service = service;
    }

    // Lista las imágenes registradas del vehículo.
    [HttpGet]
    public async Task<IActionResult> GetImagesAsync(int vehicleId)
    {
        var result = await _service.GetImagesAsync(vehicleId);
        return StatusCode(result.Status, result);
    }

    // Devuelve el archivo de imagen en el formato y nombre originales.
    [HttpGet("{imageId:int}/file")]
    public async Task<IActionResult> GetFileAsync(int vehicleId, int imageId)
    {
        var result = await _service.GetFileAsync(vehicleId, imageId);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return File(result.Data!.Content, result.Data.ContentType, result.Data.FileName);
    }

    // Sube una imagen al vehículo; la primera se guarda como principal.
    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes + 2 * 1024 * 1024)]
    public async Task<IActionResult> UploadAsync(int vehicleId, [FromForm] IFormFile? file, [FromForm] bool isMain = false)
    {
        var dto = await ToUploadDtoAsync(file, isMain);
        if (dto is null)
            return StatusCode(400, Result<VehicleImageDto>.Failure(400, "Bad Request",
                "Debe adjuntar un archivo de imagen no vacío (campo 'file')."));

        var result = await _service.UploadAsync(vehicleId, dto);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetVehicleById", new { id = vehicleId }, result);
    }

    // Reemplaza el archivo de una imagen del vehículo.
    [HttpPut("{imageId:int}")]
    [RequestSizeLimit(MaxFileSizeBytes + 2 * 1024 * 1024)]
    public async Task<IActionResult> ReplaceAsync(int vehicleId, int imageId, [FromForm] IFormFile? file)
    {
        var dto = await ToUploadDtoAsync(file, isMain: false);
        if (dto is null)
            return StatusCode(400, Result<VehicleImageDto>.Failure(400, "Bad Request",
                "Debe adjuntar un archivo de imagen no vacío (campo 'file')."));

        var result = await _service.ReplaceAsync(vehicleId, imageId, dto);
        return StatusCode(result.Status, result);
    }

    // Elimina la imagen y su archivo del vehículo.
    [HttpDelete("{imageId:int}")]
    public async Task<IActionResult> DeleteAsync(int vehicleId, int imageId)
    {
        var result = await _service.DeleteAsync(vehicleId, imageId);
        return StatusCode(result.Status, result);
    }

    // El servicio recibe los datos del archivo sin depender de IFormFile.
    private static async Task<VehicleImageUploadDto?> ToUploadDtoAsync(IFormFile? file, bool isMain)
    {
        if (file is null || file.Length == 0)
            return null;

        await using var memory = new MemoryStream();
        await file.CopyToAsync(memory);

        return new VehicleImageUploadDto
        {
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            Content = memory.ToArray(),
            SizeBytes = file.Length,
            IsMain = isMain
        };
    }
}
