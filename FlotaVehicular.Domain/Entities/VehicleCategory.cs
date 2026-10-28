namespace FlotaVehicular.Domain.Entities;

// Categoría a la que pertenecen los vehículos y que define su mantenimiento preventivo.
public class VehicleCategory
{
    // Identificador único de la categoría.
    public int IdVehicleCategory { get; set; }

    // Nombre de la categoría; es único en la base de datos.
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Kilómetros que deben transcurrir entre un mantenimiento preventivo y el siguiente.
    public int PreventiveMaintenanceIntervalKm { get; set; }

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
