namespace FlotaVehicular.Domain.Entities;

public class MaintenanceOrder
{
    public int IdMaintenanceOrder { get; set; }

    public string OrderCode { get; set; } = null!;

    public int IdVehicle { get; set; }

    public MaintenanceType MaintenanceType { get; set; }

    // No toda orden requiere una especialidad desde su creación.
    public int? IdRequiredSpecialty { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime RequestDate { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    public OrderPriority Priority { get; set; }

    public decimal TotalCost { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
    public virtual Specialty? RequiredSpecialty { get; set; }
}
