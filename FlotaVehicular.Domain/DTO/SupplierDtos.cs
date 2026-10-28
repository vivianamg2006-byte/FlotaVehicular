using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.DTO;

// Datos para registrar un proveedor.
public record class SupplierCreateDto
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "El contacto es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El contacto debe tener entre 3 y 150 caracteres.")]
    public string? Contact { get; set; }

    [StringLength(250, ErrorMessage = "La dirección no puede superar 250 caracteres.")]
    public string? Address { get; set; }

    [StringLength(2000, ErrorMessage = "Las condiciones generales no pueden superar 2000 caracteres.")]
    public string? GeneralConditions { get; set; }
}

// Datos para actualizar un proveedor existente.
public record class SupplierUpdateDto
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "El contacto es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El contacto debe tener entre 3 y 150 caracteres.")]
    public string? Contact { get; set; }

    [StringLength(250, ErrorMessage = "La dirección no puede superar 250 caracteres.")]
    public string? Address { get; set; }

    [StringLength(2000, ErrorMessage = "Las condiciones generales no pueden superar 2000 caracteres.")]
    public string? GeneralConditions { get; set; }
}

// Proveedor en la vista resumida de los listados.
public record class SupplierListDto
{
    public int IdSupplier { get; set; }
    public string Name { get; set; } = "";
    public string Contact { get; set; } = "";
    public string? Address { get; set; }
    public string? GeneralConditions { get; set; }
    public bool Status { get; set; }
    public int SparePartCount { get; set; }
}

// Proveedor con el detalle de sus repuestos y ofertas.
public record class SupplierDetailDto
{
    public int IdSupplier { get; set; }
    public string Name { get; set; } = "";
    public string Contact { get; set; } = "";
    public string? Address { get; set; }
    public string? GeneralConditions { get; set; }
    public bool Status { get; set; }
    public List<SparePartSupplierListDto> SpareParts { get; set; } = new();
    public int SparePartCount { get; set; }
}

// Filtros y paginación para consultar proveedores.
public record class SupplierQueryDto
{
    [StringLength(100, ErrorMessage = "El texto de búsqueda no puede superar 100 caracteres.")]
    public string? Search { get; set; }

    public bool IncludeInactive { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
