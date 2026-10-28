namespace FlotaVehicular.Domain.Entities;

// Unidad de la flota con su contexto de operación y mantenimiento preventivo.
public class Vehicle
{
    // Identificador único de la unidad.
    public int IdVehicle { get; set; }

    // Código de la unidad, por ejemplo VEH-0001; es único en la base de datos.
    public string Code { get; set; } = null!;

    // Placa de la unidad, única; se guarda en mayúsculas y sin espacios al inicio o al final.
    public string LicensePlate { get; set; } = null!;

    // Nombre o descripción corta de la unidad.
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Fecha en que se adquirió la unidad.
    public DateTime AcquisitionDate { get; set; }

    // Valor de referencia de compra de la unidad.
    public decimal ReferenceValue { get; set; }

    // Identificador de la categoría a la que pertenece la unidad.
    public int IdVehicleCategory { get; set; }

    // Ruta de la imagen principal que guarda el módulo de imágenes.
    public string? ImagePath { get; set; }

    // Marca del vehículo.
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }

    // Kilometraje actual recorrido por la unidad.
    public int CurrentMileage { get; set; }

    // Kilometraje al que se realizó el último mantenimiento preventivo.
    public int LastPreventiveMaintenanceMileage { get; set; }

    // Estado actual de la unidad.
    public VehicleStatus Status { get; set; }

    // Marca de tiempo de creación de la unidad.
    public DateTime CreatedAt { get; set; }

    // Se completa cuando se da de baja la unidad; conserva el registro.
    public DateTime? DeactivationDate { get; set; }

    public virtual VehicleCategory Category { get; set; } = null!;
    public virtual ICollection<VehicleImage> Images { get; set; } = new List<VehicleImage>();
}
