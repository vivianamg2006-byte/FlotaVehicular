namespace FlotaVehicular.Domain.Entities;

public class Technician
{
    public int IdTechnician { get; set; }
    public string FullName { get; set; } = null!;
    public string IdentificationNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }

    public bool IsAvailable { get; set; }

    public decimal HourlyRate { get; set; }

    // La baja es lógica para conservar el historial de órdenes.
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
