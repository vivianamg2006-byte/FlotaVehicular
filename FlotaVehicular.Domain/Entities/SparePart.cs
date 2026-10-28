namespace FlotaVehicular.Domain.Entities;

// Catálogo de repuestos con su nivel de stock y proveedores asociados.
public class SparePart
{
    // Identificador único del repuesto.
    public int IdSparePart { get; set; }

    // Código del repuesto, por ejemplo REP-001; es único.
    public string Code { get; set; } = null!;

    // Nombre del repuesto.
    public string Name { get; set; } = null!;

    // Unidad de medida, por ejemplo Unidad o Litro.
    public string UnitOfMeasure { get; set; } = null!;

    // Cantidad disponible en el almacén.
    public int CurrentStock { get; set; }

    // Cantidad mínima antes de considerar que el repuesto está en riesgo.
    public int MinStock { get; set; }

    public virtual ICollection<SparePartSupplier> SparePartSuppliers { get; set; } = new List<SparePartSupplier>();
}
