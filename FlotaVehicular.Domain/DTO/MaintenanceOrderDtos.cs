using System.ComponentModel.DataAnnotations;
using FlotaVehicular.Domain.Entities;

namespace FlotaVehicular.Domain.DTO;

public record class MaintenanceOrderCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El vehículo es obligatorio.")]
    public int IdVehicle { get; set; }

    [Required(ErrorMessage = "El tipo de mantenimiento es obligatorio.")]
    public MaintenanceType? MaintenanceType { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La especialidad indicada no es válida.")]
    public int? IdRequiredSpecialty { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre 5 y 500 caracteres.")]
    public string? Reason { get; set; }

    public DateTime? ScheduledDate { get; set; }

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    public OrderPriority? Priority { get; set; }
}

public record class MaintenanceOrderListDto
{
    public int IdMaintenanceOrder { get; set; }
    public string OrderCode { get; set; } = "";
    public int IdVehicle { get; set; }
    public string VehicleCode { get; set; } = "";
    public string VehicleLicensePlate { get; set; } = "";
    public string MaintenanceType { get; set; } = "";
    public int? IdRequiredSpecialty { get; set; }
    public string? RequiredSpecialtyName { get; set; }
    public string Reason { get; set; } = "";
    public DateTime RequestDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string Priority { get; set; } = "";
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public record class MaintenanceOrderDetailDto
{
    public int IdMaintenanceOrder { get; set; }
    public string OrderCode { get; set; } = "";
    public int IdVehicle { get; set; }
    public string VehicleCode { get; set; } = "";
    public string VehicleLicensePlate { get; set; } = "";
    public string VehicleName { get; set; } = "";
    public string VehicleStatus { get; set; } = "";
    public string MaintenanceType { get; set; } = "";
    public int? IdRequiredSpecialty { get; set; }
    public string? RequiredSpecialtyName { get; set; }
    public string Reason { get; set; } = "";
    public DateTime RequestDate { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string Priority { get; set; } = "";
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public record class MaintenanceOrderQueryDto
{
    public OrderStatus? Status { get; set; }
    public int? IdVehicle { get; set; }
    public MaintenanceType? MaintenanceType { get; set; }
    public OrderPriority? Priority { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
