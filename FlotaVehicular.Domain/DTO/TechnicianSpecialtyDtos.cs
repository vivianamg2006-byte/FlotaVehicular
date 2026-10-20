using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.DTO;

public record class TechnicianSpecialtyCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El técnico es obligatorio.")]
    public int IdTechnician { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La especialidad es obligatoria.")]
    public int IdSpecialty { get; set; }

    [Required(ErrorMessage = "El nivel de certificación es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nivel de certificación debe tener entre 2 y 50 caracteres.")]
    public string? CertificationLevel { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime SinceDate { get; set; }
}

public record class TechnicianSpecialtyUpdateDto
{
    [Required(ErrorMessage = "El nivel de certificación es obligatorio.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nivel de certificación debe tener entre 2 y 50 caracteres.")]
    public string? CertificationLevel { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime SinceDate { get; set; }
}

public record class TechnicianSpecialtyListDto
{
    public int IdTechnician { get; set; }
    public string TechnicianName { get; set; } = "";
    public string IdentificationNumber { get; set; } = "";
    public bool TechnicianActive { get; set; }
    public int IdSpecialty { get; set; }
    public string SpecialtyName { get; set; } = "";
    public string CertificationLevel { get; set; } = "";
    public DateTime SinceDate { get; set; }
}

public record class TechnicianSpecialtyQueryDto
{
    public int? IdTechnician { get; set; }
    public int? IdSpecialty { get; set; }

    [StringLength(100, ErrorMessage = "El texto de búsqueda no puede superar 100 caracteres.")]
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
