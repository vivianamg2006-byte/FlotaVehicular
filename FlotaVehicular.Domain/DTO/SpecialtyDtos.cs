using System.ComponentModel.DataAnnotations;

namespace FlotaVehicular.Domain.DTO;

public record class SpecialtyCreateDto
{
    [Required(ErrorMessage = "El nombre de la especialidad es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Description { get; set; }
}

public record class SpecialtyUpdateDto
{
    [Required(ErrorMessage = "El nombre de la especialidad es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string? Name { get; set; }

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Description { get; set; }
}

public record class SpecialtyListDto
{
    public int IdSpecialty { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int TechnicianCount { get; set; }
}

public record class SpecialtyDetailDto
{
    public int IdSpecialty { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<TechnicianSpecialtyListDto> Technicians { get; set; } = new();
    public int TechnicianCount { get; set; }
}

public record class SpecialtyQueryDto
{
    [StringLength(100, ErrorMessage = "El texto de búsqueda no puede superar 100 caracteres.")]
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
