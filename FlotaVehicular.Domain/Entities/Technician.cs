namespace FlotaVehicular.Domain.Entities;

// Técnico que atiende las órdenes de mantenimiento de la flota.
public class Technician
{
    // Identificador único del técnico.
    public int IdTechnician { get; set; }
    // Nombre completo del técnico.
    public string FullName { get; set; } = null!;
    // Número de identificación o documento del técnico; es único.
    public string IdentificationNumber { get; set; } = null!;
    // Correo del técnico; es único y debe tener formato válido.
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }

    // Indica si el técnico está disponible para recibir órdenes.
    public bool IsAvailable { get; set; }

    // Tarifa por hora que cobra el técnico.
    public decimal HourlyRate { get; set; }

    // La baja es lógica para conservar el historial de órdenes.
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TechnicianSpecialty> TechnicianSpecialties { get; set; } = new List<TechnicianSpecialty>();
}
