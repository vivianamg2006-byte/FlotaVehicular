namespace FlotaVehicular.Domain.Entities;

// Relación N:N entre un repuesto y un proveedor, con las condiciones de esa oferta.
public class SparePartSupplier
{
    // Identificador del repuesto ofertado.
    public int IdSparePart { get; set; }

    // Identificador del proveedor al que se ofrece el repuesto.
    public int IdSupplier { get; set; }

    // El precio y el plazo pertenecen a esta oferta, no al catálogo de repuestos.
    public decimal OfferedPrice { get; set; }

    public int EstimatedDeliveryDays { get; set; }

    public DateTime AssociatedAt { get; set; }

    public virtual SparePart SparePart { get; set; } = null!;
    public virtual Supplier Supplier { get; set; } = null!;
}
