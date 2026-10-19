using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.DTO;

public record class VehicleCategoryCreateDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Description { get; set; }

    [Range(1, 1_000_000, ErrorMessage = "El intervalo de mantenimiento preventivo debe estar entre 1 y 1000000 km.")]
    public int PreventiveMaintenanceIntervalKm { get; set; }
}

public record class VehicleCategoryUpdateDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Description { get; set; }

    [Range(1, 1_000_000, ErrorMessage = "El intervalo de mantenimiento preventivo debe estar entre 1 y 1000000 km.")]
    public int PreventiveMaintenanceIntervalKm { get; set; }
}

public record class VehicleCategoryListDto
{
    public int IdVehicleCategory { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int PreventiveMaintenanceIntervalKm { get; set; }
    public int VehicleCount { get; set; }
}

public record class VehicleCategoryDetailDto
{
    public int IdVehicleCategory { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int PreventiveMaintenanceIntervalKm { get; set; }
    public int VehicleCount { get; set; }

    public List<string> VehicleCodes { get; set; } = new();
}

public record class VehicleCategoryQueryDto
{
    [StringLength(80, ErrorMessage = "El texto de búsqueda no puede superar 80 caracteres.")]
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
