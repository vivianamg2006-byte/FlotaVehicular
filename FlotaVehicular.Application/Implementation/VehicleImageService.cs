using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;
using SkiaSharp;

namespace FlotaVehicular.Application.Implementation;

// Casos de uso del módulo de imágenes de vehículos.
public class VehicleImageService : IVehicleImageService
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
        { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private readonly IVehicleImageRepository _repository;
    private readonly IImageStorage _storage;
    private readonly IMapper _mapper;

    public VehicleImageService(IVehicleImageRepository repository, IImageStorage storage, IMapper mapper)
    {
        _repository = repository;
        _storage = storage;
        _mapper = mapper;
    }

    // Lista las imágenes registradas del vehículo.
    public async Task<Result<IEnumerable<VehicleImageDto>>> GetImagesAsync(int vehicleId)
    {
        if (await _repository.GetVehicleForUpdateAsync(vehicleId) is null)
            return Result<IEnumerable<VehicleImageDto>>.Failure(404, "Not Found",
                $"El vehículo {vehicleId} no existe.");

        var images = await _repository.GetByVehicleAsync(vehicleId);
        var data = _mapper.Map<IEnumerable<VehicleImageDto>>(images);

        return Result<IEnumerable<VehicleImageDto>>.Success(data);
    }

    // Sube una imagen al vehículo.
    public async Task<Result<VehicleImageDto>> UploadAsync(int vehicleId, VehicleImageUploadDto image)
    {
        var vehicle = await _repository.GetVehicleForUpdateAsync(vehicleId);
        if (vehicle is null)
            return Result<VehicleImageDto>.Failure(404, "Not Found", $"El vehículo {vehicleId} no existe.");

        var validationError = Validate(image);
        if (validationError is not null)
            return Result<VehicleImageDto>.Failure(
                validationError.Value.Status, validationError.Value.Reason, validationError.Value.Message);

        var relativePath = await _storage.SaveAsync(image.FileName!, image.Content!);

        var existing = await _repository.GetTrackedByVehicleAsync(vehicleId);
        // La primera imagen se usa como principal.
        var makeMain = image.IsMain || !existing.Any();

        var entity = new VehicleImage
        {
            IdVehicle = vehicleId,
            FileName = Path.GetFileName(relativePath),
            OriginalFileName = Path.GetFileName(image.FileName!),
            ContentType = image.ContentType!,
            SizeBytes = image.Content!.LongLength,
            RelativePath = relativePath,
            PublicUrl = _storage.ToPublicUrl(relativePath),
            UploadedAt = DateTime.UtcNow,
            IsMain = makeMain
        };

        if (makeMain)
        {
            foreach (var other in existing.Where(i => i.IsMain))
                other.IsMain = false;

            vehicle.ImagePath = entity.PublicUrl;
        }

        try
        {
            await _repository.AddAsync(entity);
        }
        catch (Exception)
        {
            // Si falla el registro, no dejamos el archivo nuevo en disco.
            await _storage.DeleteAsync(relativePath);

            return Result<VehicleImageDto>.Failure(500, "Internal Server Error",
                "No se pudo registrar la imagen en la base de datos; no se guardó ningún archivo.");
        }

        var data = _mapper.Map<VehicleImageDto>(entity);

        return Result<VehicleImageDto>.Success(data, 201, "Created", "Imagen subida.");
    }

    // Reemplaza el archivo de una imagen del vehículo.
    public async Task<Result<VehicleImageDto>> ReplaceAsync(int vehicleId, int imageId, VehicleImageUploadDto image)
    {
        var vehicle = await _repository.GetVehicleForUpdateAsync(vehicleId);
        if (vehicle is null)
            return Result<VehicleImageDto>.Failure(404, "Not Found", $"El vehículo {vehicleId} no existe.");

        var trackedImages = (await _repository.GetTrackedByVehicleAsync(vehicleId)).ToList();
        var entity = trackedImages.FirstOrDefault(i => i.IdVehicleImage == imageId);

        if (entity is null)
            return Result<VehicleImageDto>.Failure(404, "Not Found",
                $"La imagen {imageId} no pertenece al vehículo {vehicleId}.");

        var validationError = Validate(image);
        if (validationError is not null)
            return Result<VehicleImageDto>.Failure(
                validationError.Value.Status, validationError.Value.Reason, validationError.Value.Message);

        // El archivo anterior se conserva hasta que se guarde el reemplazo.
        var oldRelativePath = entity.RelativePath;
        var oldVehicleImagePath = vehicle.ImagePath;
        var previous = new
        {
            entity.FileName,
            entity.OriginalFileName,
            entity.ContentType,
            entity.SizeBytes,
            entity.RelativePath,
            entity.PublicUrl,
            entity.UploadedAt
        };

        var newRelativePath = await _storage.SaveAsync(image.FileName!, image.Content!);

        entity.FileName = Path.GetFileName(newRelativePath);
        entity.OriginalFileName = Path.GetFileName(image.FileName!);
        entity.ContentType = image.ContentType!;
        entity.SizeBytes = image.Content!.LongLength;
        entity.RelativePath = newRelativePath;
        entity.PublicUrl = _storage.ToPublicUrl(newRelativePath);
        entity.UploadedAt = DateTime.UtcNow;

        if (entity.IsMain)
            vehicle.ImagePath = entity.PublicUrl;

        try
        {
            await _repository.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Si falla la base de datos, se descarta el archivo nuevo.
            await _storage.DeleteAsync(newRelativePath);

            entity.FileName = previous.FileName;
            entity.OriginalFileName = previous.OriginalFileName;
            entity.ContentType = previous.ContentType;
            entity.SizeBytes = previous.SizeBytes;
            entity.RelativePath = previous.RelativePath;
            entity.PublicUrl = previous.PublicUrl;
            entity.UploadedAt = previous.UploadedAt;
            vehicle.ImagePath = oldVehicleImagePath;

            return Result<VehicleImageDto>.Failure(500, "Internal Server Error",
                "No se pudo guardar el reemplazo en la base de datos; se conserva la imagen anterior.");
        }

        // La base ya apunta al nuevo archivo; ahora se puede borrar el anterior.
        await _storage.DeleteAsync(oldRelativePath);

        var data = _mapper.Map<VehicleImageDto>(entity);

        return Result<VehicleImageDto>.Success(data, 200, "OK", "Imagen reemplazada.");
    }

    // Elimina la imagen y su archivo del vehículo.
    public async Task<Result<int>> DeleteAsync(int vehicleId, int imageId)
    {
        var vehicle = await _repository.GetVehicleForUpdateAsync(vehicleId);
        if (vehicle is null)
            return Result<int>.Failure(404, "Not Found", $"El vehículo {vehicleId} no existe.");

        var trackedImages = (await _repository.GetTrackedByVehicleAsync(vehicleId)).ToList();
        var entity = trackedImages.FirstOrDefault(i => i.IdVehicleImage == imageId);

        if (entity is null)
            return Result<int>.Failure(404, "Not Found",
                $"La imagen {imageId} no pertenece al vehículo {vehicleId}.");

        var wasMain = entity.IsMain;
        var relativePath = entity.RelativePath;

        _repository.Remove(entity);
        await _repository.SaveChangesAsync();

        await _storage.DeleteAsync(relativePath);

        // Al quitar la principal se elige otra de las imágenes del vehículo.
        if (wasMain)
        {
            var remaining = await _repository.GetTrackedByVehicleAsync(vehicleId);
            var newMain = remaining.FirstOrDefault();

            if (newMain is not null)
            {
                newMain.IsMain = true;
                vehicle.ImagePath = newMain.PublicUrl;
                await _repository.SaveChangesAsync();
            }
            else
            {
                vehicle.ImagePath = null;
                await _repository.SaveChangesAsync();
            }
        }

        return Result<int>.Success(imageId, 200, "OK", "Imagen eliminada.");
    }

    // Devuelve el archivo de imagen en el formato y nombre originales.
    public async Task<Result<VehicleImageFileDto>> GetFileAsync(int vehicleId, int imageId)
    {
        var image = await _repository.GetByIdAsync(vehicleId, imageId);

        if (image is null)
            return Result<VehicleImageFileDto>.Failure(404, "Not Found",
                $"La imagen {imageId} no pertenece al vehículo {vehicleId}.");

        try
        {
            var content = await _storage.ReadAllBytesAsync(image.RelativePath);

            var data = new VehicleImageFileDto
            {
                Content = content,
                ContentType = image.ContentType,
                FileName = image.OriginalFileName
            };

            return Result<VehicleImageFileDto>.Success(data);
        }
        catch (Exception)
        {
            return Result<VehicleImageFileDto>.Failure(410, "Gone",
                "El archivo de la imagen ya no está disponible en el servidor.");
        }
    }

    private static (int Status, string Reason, string Message)? Validate(VehicleImageUploadDto image)
    {
        if (image.Content is null || image.Content.Length == 0)
            return (400, "Bad Request", "El archivo está vacío.");

        if (image.Content.Length > MaxFileSizeBytes)
            return (413, "Payload Too Large",
                $"El archivo supera el tamaño máximo de {MaxFileSizeBytes / (1024 * 1024)} MB.");

        var contentType = (image.ContentType ?? string.Empty).Trim().ToLowerInvariant();
        if (!AllowedContentTypes.Contains(contentType))
            return (400, "Bad Request",
                $"Tipo de contenido no permitido: '{image.ContentType}'. Use uno de: {string.Join(", ", AllowedContentTypes)}.");

        var extension = Path.GetExtension(image.FileName ?? string.Empty).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return (400, "Bad Request", $"Extensión no permitida: '{extension}'.");

        // Abrir la imagen permite comprobar su contenido, no solo el nombre del archivo.
        using var stream = new SKMemoryStream(image.Content);
        using var codec = SKCodec.Create(stream);
        if (codec is null)
            return (400, "Bad Request",
                "El contenido del archivo no corresponde a una imagen válida (firma de formato no reconocida).");

        var expectedType = codec.EncodedFormat switch
        {
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Gif => "image/gif",
            SKEncodedImageFormat.Webp => "image/webp",
            _ => null
        };

        if (expectedType is null)
            return (400, "Bad Request", "El formato de la imagen no está permitido.");

        var formatName = codec.EncodedFormat.ToString().ToUpperInvariant();
        if (contentType != expectedType)
            return (400, "Bad Request",
                $"El contenido real es {formatName} pero el tipo declarado es '{image.ContentType}'.");

        var expectedExtension = "." + codec.EncodedFormat.ToString().ToLowerInvariant();
        if (extension != expectedExtension && !(codec.EncodedFormat == SKEncodedImageFormat.Jpeg && extension == ".jpg"))
            return (400, "Bad Request",
                $"El contenido real es {formatName} pero la extensión es '{extension}'.");

        // Un archivo pequeño también puede declarar dimensiones demasiado grandes.
        if (codec.Info.Width <= 0 || codec.Info.Height <= 0 || (long)codec.Info.Width * codec.Info.Height > 20_000_000)
            return (400, "Bad Request", "La imagen debe tener como máximo 20 millones de píxeles.");

        using var bitmap = new SKBitmap(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) != SKCodecResult.Success)
            return (400, "Bad Request",
                "El archivo de imagen está corrupto o incompleto.");

        return null;
    }
}
