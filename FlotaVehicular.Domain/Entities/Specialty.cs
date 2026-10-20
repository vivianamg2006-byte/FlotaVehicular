namespace FlotaVehicular.Domain.Entities;

public class Specialty
{
    public int IdSpecialty { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public virtual ICollection<TechnicianSpecialty> TechnicianSpecialties { get; set; } = new List<TechnicianSpecialty>();
}
