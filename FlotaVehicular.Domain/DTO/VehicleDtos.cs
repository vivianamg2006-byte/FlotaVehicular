using System.ComponentModel.DataAnnotations;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Domain.Validations;

namespace FlotaVehicular.Domain.DTO;

// Datos para registrar un vehículo nuevo.
public record class VehicleCreateDto
{
    [Required(ErrorMessage = "El código del vehículo es obligatorio.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 20 caracteres.")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(15, MinimumLength = 2, ErrorMessage = "La placa debe tener entre 2 y 15 caracteres.")]
    public string? LicensePlate { get; set; }

    [Required(ErrorMessage = "El nombre del vehículo es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La fecha de adquisición es obligatoria.")]
    public DateTime? AcquisitionDate { get; set; }

    [Range(0, 99_999_999.99, ErrorMessage = "El valor de referencia no puede ser negativo.")]
    public decimal ReferenceValue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria.")]
    public int IdVehicleCategory { get; set; }

    [StringLength(300, ErrorMessage = "La ruta de la imagen no puede superar 300 caracteres.")]
    [AllowedExtensions(".jpg", ".jpeg", ".png", ".webp", ".gif")]
    public string? Image { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 50 caracteres.")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El modelo no puede superar 50 caracteres.")]
    public string? Model { get; set; }

    [Range(1950, 2100, ErrorMessage = "El año debe estar entre 1950 y 2100.")]
    public int Year { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El kilometraje actual no puede ser negativo.")]
    public int CurrentMileage { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El kilometraje del último mantenimiento preventivo no puede ser negativo.")]
    public int LastPreventiveMaintenanceMileage { get; set; }
}

// Datos para actualizar un vehículo existente.
public record class VehicleUpdateDto
{
    [Required(ErrorMessage = "El código del vehículo es obligatorio.")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 20 caracteres.")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "La placa es obligatoria.")]
    [StringLength(15, MinimumLength = 2, ErrorMessage = "La placa debe tener entre 2 y 15 caracteres.")]
    public string? LicensePlate { get; set; }

    [Required(ErrorMessage = "El nombre del vehículo es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La fecha de adquisición es obligatoria.")]
    public DateTime? AcquisitionDate { get; set; }

    [Range(0, 99_999_999.99, ErrorMessage = "El valor de referencia no puede ser negativo.")]
    public decimal ReferenceValue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria.")]
    public int IdVehicleCategory { get; set; }

    [StringLength(300, ErrorMessage = "La ruta de la imagen no puede superar 300 caracteres.")]
    [AllowedExtensions(".jpg", ".jpeg", ".png", ".webp", ".gif")]
    public string? Image { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 50 caracteres.")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "El modelo no puede superar 50 caracteres.")]
    public string? Model { get; set; }

    [Range(1950, 2100, ErrorMessage = "El año debe estar entre 1950 y 2100.")]
    public int Year { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El kilometraje actual no puede ser negativo.")]
    public int CurrentMileage { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El kilometraje del último mantenimiento preventivo no puede ser negativo.")]
    public int LastPreventiveMaintenanceMileage { get; set; }
}

// Estado al que se cambia un vehículo mediante su endpoint.
public record class VehicleStatusDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public VehicleStatus? Status { get; set; }
}

// Vehículo en la vista resumida de los listados.
public record class VehicleListDto
{
    public int IdVehicle { get; set; }
    public string Code { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public decimal ReferenceValue { get; set; }
    public int IdVehicleCategory { get; set; }
    // Nombre de la categoría a la que pertenece la unidad.
    public string CategoryName { get; set; } = "";
    public string? ImagePath { get; set; }
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public int CurrentMileage { get; set; }
    public int LastPreventiveMaintenanceMileage { get; set; }
    public VehicleStatus Status { get; set; }
    // Nombre legible del estado de la unidad.
    public string StatusName { get; set; } = "";

    // Kilómetros que faltan para el próximo mantenimiento preventivo.
    public int KmToNextPreventiveMaintenance { get; set; }
}

// Vehículo con el detalle completo: categoría, imágenes y kilometraje calculado.
public record class VehicleDetailDto
{
    public int IdVehicle { get; set; }
    public string Code { get; set; } = "";
    public string LicensePlate { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public decimal ReferenceValue { get; set; }
    public int IdVehicleCategory { get; set; }
    public VehicleCategoryDetailDto Category { get; set; } = new();
    public string? ImagePath { get; set; }
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public int CurrentMileage { get; set; }
    public int LastPreventiveMaintenanceMileage { get; set; }
    public VehicleStatus Status { get; set; }
    // Nombre legible del estado de la unidad.
    public string StatusName { get; set; } = "";
    public int KmToNextPreventiveMaintenance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeactivationDate { get; set; }
    public List<VehicleImageDto> Images { get; set; } = new();
    // Número de imágenes registradas para el vehículo.
    public int ImageCount { get; set; }
}

// Filtros y paginación para consultar vehículos.
public record class VehicleQueryDto
{
    [StringLength(100, ErrorMessage = "El texto de búsqueda no puede superar 100 caracteres.")]
    public string? Search { get; set; }

    public int? IdVehicleCategory { get; set; }
    public VehicleStatus? Status { get; set; }

    // Permite consultar también los vehículos dados de baja.
    public bool IncludeDeactivated { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
