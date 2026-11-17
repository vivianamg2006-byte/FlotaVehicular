namespace FlotaVehicular.Domain.Entities;

// Solicitud de mantenimiento que una unidad le pide a la flota.
public class MaintenanceOrder
{
    // Identificador único de la orden.
    public int IdMaintenanceOrder { get; set; }

    // Código de la orden, por ejemplo OM-20261001-0001; es único.
    public string OrderCode { get; set; } = null!;

    // Identificador de la unidad que se atiende.
    public int IdVehicle { get; set; }

    // Tipo de trabajo solicitado: Preventivo, Correctivo o Emergencia.
    public MaintenanceType MaintenanceType { get; set; }

    // No toda orden requiere una especialidad desde su creación.
    public int? IdRequiredSpecialty { get; set; }

    // Motivo o descripción del trabajo solicitado.
    public string Reason { get; set; } = null!;

    // Fecha en que se registró la solicitud.
    public DateTime RequestDate { get; set; }

    // Fecha programada para ejecutar la orden; es opcional.
    public DateTime? ScheduledDate { get; set; }

    // Fecha en que se cerró la orden; se llena al completarla.
    public DateTime? ClosedDate { get; set; }

    // Prioridad con la que se atiende la orden.
    public OrderPriority Priority { get; set; }

    // Costo total estimado o registrado para la orden.
    public decimal TotalCost { get; set; }

    // Estado actual de la orden dentro de su ciclo de vida.
    public OrderStatus Status { get; set; }

    // Marca de tiempo de creación de la orden.
    public DateTime CreatedAt { get; set; }

    // Usuario que registró la solicitud; es opcional para las órdenes históricas.
    public int? CreatedByUserId { get; set; }

    // Unidad que recibe el mantenimiento.
    public virtual Vehicle Vehicle { get; set; } = null!;

    // Especialidad requerida para el trabajo, cuando aplica.
    public virtual Specialty? RequiredSpecialty { get; set; }

    // Usuario autenticado que creó la orden.
    public virtual User? CreatedByUser { get; set; }
}
