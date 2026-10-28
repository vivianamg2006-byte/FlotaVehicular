namespace FlotaVehicular.Domain.Entities;

// Especialidad técnica con la que se pueden atender las órdenes.
public class Specialty
{
    // Identificador único de la especialidad.
    public int IdSpecialty { get; set; }

    // Nombre de la especialidad; es único.
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TechnicianSpecialty> TechnicianSpecialties { get; set; } = new List<TechnicianSpecialty>();
    public virtual ICollection<MaintenanceOrder> MaintenanceOrders { get; set; } = new List<MaintenanceOrder>();
}
