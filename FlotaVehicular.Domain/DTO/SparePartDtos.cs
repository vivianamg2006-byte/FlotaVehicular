using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.DTO;

public record class SparePartCreateDto
{
    [Required(ErrorMessage = "El código del repuesto es obligatorio.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 30 caracteres.")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "El nombre del repuesto es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "La unidad de medida debe tener entre 2 y 30 caracteres.")]
    public string? UnitOfMeasure { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock actual no puede ser negativo.")]
    public int CurrentStock { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int MinStock { get; set; }
}

public record class SparePartUpdateDto
{
    [Required(ErrorMessage = "El código del repuesto es obligatorio.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 30 caracteres.")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "El nombre del repuesto es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "La unidad de medida debe tener entre 2 y 30 caracteres.")]
    public string? UnitOfMeasure { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock actual no puede ser negativo.")]
    public int CurrentStock { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int MinStock { get; set; }
}

public record class SparePartListDto
{
    public int IdSparePart { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string UnitOfMeasure { get; set; } = "";
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }

    public bool IsBelowMinimum { get; set; }
    public int SupplierCount { get; set; }
}

public record class SparePartDetailDto
{
    public int IdSparePart { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string UnitOfMeasure { get; set; } = "";
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }
    public bool IsBelowMinimum { get; set; }
    public List<SparePartSupplierListDto> Suppliers { get; set; } = new();
    public int SupplierCount { get; set; }
}

public record class SparePartQueryDto
{
    [StringLength(100, ErrorMessage = "El texto de búsqueda no puede superar 100 caracteres.")]
    public string? Search { get; set; }

    public bool BelowMinimumOnly { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
