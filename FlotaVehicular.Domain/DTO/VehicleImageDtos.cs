using System.ComponentModel.DataAnnotations;
using FlotaVehicular.Domain.Validations;

namespace FlotaVehicular.Domain.DTO;

public record class VehicleImageUploadDto
{
    [Required(ErrorMessage = "El nombre del archivo es obligatorio.")]
    [StringLength(255, ErrorMessage = "El nombre del archivo no puede superar 255 caracteres.")]
    [AllowedExtensions(".jpg", ".jpeg", ".png", ".webp", ".gif")]
    public string? FileName { get; set; }

    [Required(ErrorMessage = "El tipo de contenido (Content-Type) es obligatorio.")]
    [StringLength(100, ErrorMessage = "El tipo de contenido no puede superar 100 caracteres.")]
    public string? ContentType { get; set; }

    [Required(ErrorMessage = "El contenido del archivo es obligatorio.")]
    public byte[]? Content { get; set; }

    [Range(1, 5 * 1024 * 1024, ErrorMessage = "El archivo debe pesar entre 1 byte y 5 MB.")]
    public long SizeBytes { get; set; }

    public bool IsMain { get; set; }
}

public record class VehicleImageDto
{
    public int IdVehicleImage { get; set; }
    public int IdVehicle { get; set; }
    public string FileName { get; set; } = "";
    public string OriginalFileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }
    public string PublicUrl { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public bool IsMain { get; set; }
}

public record class VehicleImageFileDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = "";
}
