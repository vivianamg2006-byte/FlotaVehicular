namespace FlotaVehicular.Domain.Entities;

public class VehicleImage
{
    public int IdVehicleImage { get; set; }
    public int IdVehicle { get; set; }

    public string FileName { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long SizeBytes { get; set; }

    public string RelativePath { get; set; } = null!;

    public string PublicUrl { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public bool IsMain { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
}
