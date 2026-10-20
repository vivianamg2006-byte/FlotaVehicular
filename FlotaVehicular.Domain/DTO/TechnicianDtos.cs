using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.DTO;

public record class TechnicianCreateDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 100 caracteres.")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [StringLength(30, MinimumLength = 5, ErrorMessage = "La identificación debe tener entre 5 y 30 caracteres.")]
    public string? IdentificationNumber { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    public string? Phone { get; set; }

    public bool IsAvailable { get; set; } = true;

    [Required(ErrorMessage = "La tarifa por hora es obligatoria.")]
    [Range(0.01, 999_999.99, ErrorMessage = "La tarifa por hora debe ser mayor que 0 y no superar 999999.99.")]
    public decimal HourlyRate { get; set; }
}

public record class TechnicianUpdateDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre completo debe tener entre 3 y 100 caracteres.")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [StringLength(30, MinimumLength = 5, ErrorMessage = "La identificación debe tener entre 5 y 30 caracteres.")]
    public string? IdentificationNumber { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    public string? Phone { get; set; }

    public bool IsAvailable { get; set; } = true;

    [Required(ErrorMessage = "La tarifa por hora es obligatoria.")]
    [Range(0.01, 999_999.99, ErrorMessage = "La tarifa por hora debe ser mayor que 0 y no superar 999999.99.")]
    public decimal HourlyRate { get; set; }
}

public record class TechnicianListDto
{
    public int IdTechnician { get; set; }
    public string FullName { get; set; } = "";
    public string IdentificationNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public bool IsAvailable { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SpecialtyCount { get; set; }
}

public record class TechnicianDetailDto
{
    public int IdTechnician { get; set; }
    public string FullName { get; set; } = "";
    public string IdentificationNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public bool IsAvailable { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TechnicianSpecialtyListDto> Specialties { get; set; } = new();
    public int SpecialtyCount { get; set; }
}

public record class TechnicianQueryDto
{
    [StringLength(100, ErrorMessage = "El texto de búsqueda no puede superar 100 caracteres.")]
    public string? Search { get; set; }

    public bool IncludeInactive { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
