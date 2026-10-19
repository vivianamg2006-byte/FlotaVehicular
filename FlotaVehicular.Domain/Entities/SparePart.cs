namespace FlotaVehicular.Domain.Entities;

public class SparePart
{
    public int IdSparePart { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string UnitOfMeasure { get; set; } = null!;

    public int CurrentStock { get; set; }

    public int MinStock { get; set; }

    public virtual ICollection<SparePartSupplier> SparePartSuppliers { get; set; } = new List<SparePartSupplier>();
}
