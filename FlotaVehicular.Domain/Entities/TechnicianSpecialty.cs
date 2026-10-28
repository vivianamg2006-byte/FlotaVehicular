namespace FlotaVehicular.Domain.Entities;

// Relación N:N entre técnico y especialidad con su nivel de certificación.
public class TechnicianSpecialty
{
    // Identificador del técnico asociado.
    public int IdTechnician { get; set; }
    public int IdSpecialty { get; set; }

    // La certificación y la fecha pertenecen a esta relación, no al catálogo.
    public string CertificationLevel { get; set; } = null!;

    public DateTime SinceDate { get; set; }

    public virtual Technician Technician { get; set; } = null!;
    public virtual Specialty Specialty { get; set; } = null!;
}
