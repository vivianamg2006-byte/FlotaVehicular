namespace FlotaVehicular.Domain.Entities;

public class Vehicle
{
    public int IdVehicle { get; set; }

    public string Code { get; set; } = null!;

    public string LicensePlate { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime AcquisitionDate { get; set; }

    public decimal ReferenceValue { get; set; }

    public int IdVehicleCategory { get; set; }

    // Referencia a la imagen principal guardada por el módulo de imágenes.
    public string? ImagePath { get; set; }

    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }

    public int CurrentMileage { get; set; }

    public int LastPreventiveMaintenanceMileage { get; set; }

    public VehicleStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    // Se completa al dar de baja el vehículo.
    public DateTime? DeactivationDate { get; set; }

    public virtual VehicleCategory Category { get; set; } = null!;
    public virtual ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
}
