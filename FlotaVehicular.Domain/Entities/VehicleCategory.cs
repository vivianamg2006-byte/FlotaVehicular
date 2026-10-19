namespace FlotaVehicular.Domain.Entities;

public class VehicleCategory
{
    public int IdVehicleCategory { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public int PreventiveMaintenanceIntervalKm { get; set; }

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
