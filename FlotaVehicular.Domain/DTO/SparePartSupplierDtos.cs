using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.DTO;

public record class SparePartSupplierCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El repuesto es obligatorio.")]
    public int IdSparePart { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El proveedor es obligatorio.")]
    public int IdSupplier { get; set; }

    [Required(ErrorMessage = "El precio ofrecido es obligatorio.")]
    [Range(0.01, 999_999.99, ErrorMessage = "El precio ofrecido debe estar entre 0.01 y 999999.99.")]
    public decimal OfferedPrice { get; set; }

    [Range(0, 365, ErrorMessage = "El tiempo estimado de entrega debe estar entre 0 y 365 días.")]
    public int EstimatedDeliveryDays { get; set; }
}

public record class SparePartSupplierUpdateDto
{
    [Required(ErrorMessage = "El precio ofrecido es obligatorio.")]
    [Range(0.01, 999_999.99, ErrorMessage = "El precio ofrecido debe estar entre 0.01 y 999999.99.")]
    public decimal OfferedPrice { get; set; }

    [Range(0, 365, ErrorMessage = "El tiempo estimado de entrega debe estar entre 0 y 365 días.")]
    public int EstimatedDeliveryDays { get; set; }
}

public record class SparePartSupplierListDto
{
    public int IdSparePart { get; set; }
    public string SparePartCode { get; set; } = "";
    public string SparePartName { get; set; } = "";
    public int IdSupplier { get; set; }
    public string SupplierName { get; set; } = "";
    public bool SupplierActive { get; set; }
    public decimal OfferedPrice { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public DateTime AssociatedAt { get; set; }
}

public record class SparePartSupplierQueryDto
{
    public int? IdSparePart { get; set; }
    public int? IdSupplier { get; set; }

    [StringLength(100, ErrorMessage = "El texto de búsqueda no puede superar 100 caracteres.")]
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
