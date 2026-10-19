namespace FlotaVehicular.Domain.Entities;

public class SparePartSupplier
{
    public int IdSparePart { get; set; }
    public int IdSupplier { get; set; }

    // El precio y el plazo pertenecen a esta oferta, no al catálogo de repuestos.
    public decimal OfferedPrice { get; set; }

    public int EstimatedDeliveryDays { get; set; }

    public DateTime AssociatedAt { get; set; }

    public virtual SparePart SparePart { get; set; } = null!;
    public virtual Supplier Supplier { get; set; } = null!;
}
