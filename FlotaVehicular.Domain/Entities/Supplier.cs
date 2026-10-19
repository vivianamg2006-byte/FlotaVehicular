namespace FlotaVehicular.Domain.Entities;

public class Supplier
{
    public int IdSupplier { get; set; }
    public string Name { get; set; } = null!;
    public string Contact { get; set; } = null!;
    public string? Address { get; set; }

    public string? GeneralConditions { get; set; }

    public bool Status { get; set; }

    public virtual ICollection<SparePartSupplier> SparePartSuppliers { get; set; } = new List<SparePartSupplier>();
}
