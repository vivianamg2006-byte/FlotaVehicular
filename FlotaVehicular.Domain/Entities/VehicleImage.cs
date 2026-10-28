namespace FlotaVehicular.Domain.Entities;

// Imagen de un vehículo que se guarda en disco y se referencia desde el catálogo.
public class VehicleImage
{
    // Identificador único de la imagen.
    public int IdVehicleImage { get; set; }

    // Identificador del vehículo al que pertenece.
    public int IdVehicle { get; set; }

    // Nombre con el que se guarda el archivo en el servidor.
    public string FileName { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long SizeBytes { get; set; }

    public string RelativePath { get; set; } = null!;

    public string PublicUrl { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    // Indica si la imagen es la principal del vehículo.
    public bool IsMain { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
}
