using FlotaVehicular.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.Infrastructure.Data.Seed;

public static class FleetSeeder
{
    public static void Seed(DbContext context)
    {
        SeedCatalogs(context);
        context.SaveChanges();
    }

    public static async Task SeedAsync(DbContext context, CancellationToken ct = default)
    {
        SeedCatalogs(context);
        await context.SaveChangesAsync(ct);
    }

    private static void SeedCatalogs(DbContext context)
    {
        var categories = context.Set<VehicleCategory>();
        var suppliers = context.Set<Supplier>();
        var spareParts = context.Set<SparePart>();
        var vehicles = context.Set<Vehicle>();
        var links = context.Set<SparePartSupplier>();

        // Los registros se buscan por nombre o código para no duplicar la semilla.
        var categorySeeds = new (string Name, string Description, int IntervalKm)[]
        {
            ("Camión de carga",        "Camiones de carga seca y paquetería",              10000),
            ("Camioneta",              "Pick-up de una y dos toneladas",                    5000),
            ("Automóvil de pasajeros", "Vehículos para transporte de pasajeros",            5000),
            ("Autobús",                "Buses de pasajeros y rutas urbanas",               15000),
            ("Motocicleta",            "Motos de reparto y servicio",                       3000),
            ("Maquinaria pesada",      "Excavadoras, retroexcavadoras y similares",         8000),
            ("Vehículo liviano",       "Vehículos livianos de flota administrativa",        4000),
            ("Tráiler",                "Cabezales y semirremolques",                       20000),
            ("Refrigerado",            "Unidades con caja refrigerada",                     6000),
            ("Equipo de apoyo",        "Grúas, hidrolavadoras y equipos de soporte",        7000)
        };

        foreach (var seed in categorySeeds)
        {
            if (!categories.Any(c => c.Name == seed.Name))
                categories.Add(new VehicleCategory
                {
                    Name = seed.Name,
                    Description = seed.Description,
                    PreventiveMaintenanceIntervalKm = seed.IntervalKm
                });
        }

        // Proveedores
        var supplierSeeds = new (string Name, string Contact, string Address, string Conditions)[]
        {
            ("Repuestos del Norte S.A.", "ventas@repuestosnorte.com",      "Km 5 vía norte",       "Crédito a 30 días, garantía de 6 meses"),
            ("AutoPartes Central",       "Laura Méndez - 8888-1010",       "Centro urbano, local 4","Contado con 5% de descuento"),
            ("Frenos y Clutches Maxi",   "info@frenosmaxi.com",            "Zona industrial",      "Garantía de 12 meses en discos y balatas"),
            ("Lubricantes Express",      "pedidos@lubexpress.com",         "Bodega 3, polígono",   "Entrega gratuita en pedidos mayores a 500"),
            ("Distribuidora Chasis",     "Carlos Ruiz - 7777-2020",        "Av. Industrial 12",    "Crédito a 60 días para flotas"),
            ("Filtros Globales",         "contacto@filtrosglobales.com",   "Calle 8, barrio sur",  "Precio mayorista desde 10 unidades"),
            ("Baterías Power",           "Soporte Power - 6666-3030",      "Trav. 21, norte",      "Cambio de batería sin costo adicional"),
            ("Neumáticos Andina",        "comercial@neumaticosandina.com", "Ruta 27, km 3",        "Alineación gratis con compra de 4 llantas"),
            ("Motor y Transmisión Pro",  "jefatura@mtypro.com",            "Zona franca",           "Entrega especializada en 10 días hábiles"),
            ("Accesorios Flota Total",   "hola@flotatotal.com",            "Local 12, mercado",    "Soporte técnico incluido en el primer año")
        };

        foreach (var seed in supplierSeeds)
        {
            if (!suppliers.Any(s => s.Name == seed.Name))
                suppliers.Add(new Supplier
                {
                    Name = seed.Name,
                    Contact = seed.Contact,
                    Address = seed.Address,
                    GeneralConditions = seed.Conditions,
                    Status = true
                });
        }

        context.SaveChanges();

        // Repuestos
        var sparePartSeeds = new (string Code, string Name, string Unit, int CurrentStock, int MinStock)[]
        {
            ("REP-001", "Filtro de aceite",            "Unidad", 120, 30),
            ("REP-002", "Pastillas de freno delanteras","Juego",   40, 20),
            ("REP-003", "Llanta 225/70 R15",            "Unidad",  18, 12),
            ("REP-004", "Batería 12V 70Ah",             "Unidad",  15, 10),
            ("REP-005", "Correa de distribución",       "Unidad",  55, 25),
            ("REP-006", "Refrigerante orgánico",        "Litro",  200, 60),
            ("REP-007", "Aceite 15W-40",                "Litro",  900, 250),
            ("REP-008", "Amortecedor delantero",        "Unidad",  22, 16),
            ("REP-009", "Alternador 110A",               "Unidad",   7,  8),
            ("REP-010", "Kit de embrague",              "Juego",   12, 10)
        };

        foreach (var seed in sparePartSeeds)
        {
            if (!spareParts.Any(s => s.Code == seed.Code))
                spareParts.Add(new SparePart
                {
                    Code = seed.Code,
                    Name = seed.Name,
                    UnitOfMeasure = seed.Unit,
                    CurrentStock = seed.CurrentStock,
                    MinStock = seed.MinStock
                });
        }

        // Vehículos con ejemplos de los cuatro estados.
        var vehicleSeeds = new (string Code, string Plate, string Name, string Description, DateTime AcquisitionDate,
            decimal ReferenceValue, string Brand, string Model, int Year, int CurrentMileage, int LastPreventive,
            VehicleStatus Status, string CategoryName)[]
        {
            ("VEH-0001", "ABC-101", "Camión de carga 1",       "Unidad de reparto zona norte",       new DateTime(2021, 3, 15),  38500000m, "Freightliner", "Cascadia",    2020, 145000, 135000, VehicleStatus.Operativo,      "Camión de carga"),
            ("VEH-0002", "DEF-202", "Camioneta de supervisión","Supervisión de obra",                new DateTime(2022, 7, 2),   21500000m, "Toyota",       "Hilux",       2021,  78000,  74000, VehicleStatus.Operativo,      "Camioneta"),
            ("VEH-0003", "GHI-303", "Automóvil ejecutivo",     "Unidad de gerencia",                 new DateTime(2020, 11, 20), 16200000m, "Nissan",       "Altima",      2019, 112000, 107000, VehicleStatus.EnMantenimiento,"Automóvil de pasajeros"),
            ("VEH-0004", "JKL-404", "Autobús urbano",          "Ruta de personal",                   new DateTime(2019, 5, 9),   64000000m, "Mercedes-Benz","OF-1519",     2018, 486000, 472000, VehicleStatus.FueraDeServicio,"Autobús"),
            ("VEH-0005", "MNO-505", "Motocicleta de reparto",  "Entregas urbanas",                   new DateTime(2023, 2, 11),   4800000m, "Honda",        "CG 160",      2022,  34000,  31000, VehicleStatus.Operativo,      "Motocicleta"),
            ("VEH-0006", "PQR-606", "Retroexcavadora 1",       "Maquinaria de apoyo en obra",        new DateTime(2018, 9, 30),  92000000m, "Caterpillar",  "320D",        2017,   9800,   2000, VehicleStatus.EnMantenimiento,"Maquinaria pesada"),
            ("VEH-0007", "STU-707", "Automóvil administrativo","Área administrativa",                new DateTime(2022, 1, 17),  12500000m, "Kia",          "Rio",         2021,  61000,  57000, VehicleStatus.Operativo,      "Vehículo liviano"),
            ("VEH-0008", "VWX-808", "Tráiler de línea",        "Ruta larga distancia",               new DateTime(2017, 6, 25),  78000000m, "Scania",       "R450",        2016, 620000, 600000, VehicleStatus.DadoDeBaja,     "Tráiler"),
            ("VEH-0009", "YZA-909", "Camión refrigerado",      "Transporte de producto fresco",      new DateTime(2021, 10, 5),  55200000m, "Volvo",        "FE 320",      2020, 214000, 208000, VehicleStatus.Operativo,      "Refrigerado"),
            ("VEH-0010", "BCD-010", "Grúa de servicio",        "Asistencia a unidades averiadas",    new DateTime(2020, 4, 8),   41000000m, "Kenworth",     "T370",        2019,  89000,  83000, VehicleStatus.DadoDeBaja,     "Equipo de apoyo")
        };

        foreach (var seed in vehicleSeeds)
        {
            if (vehicles.Any(v => v.Code == seed.Code))
                continue;

            var category = categories.First(c => c.Name == seed.CategoryName);

            vehicles.Add(new Vehicle
            {
                Code = seed.Code,
                LicensePlate = seed.Plate,
                Name = seed.Name,
                Description = seed.Description,
                AcquisitionDate = seed.AcquisitionDate,
                ReferenceValue = seed.ReferenceValue,
                IdVehicleCategory = category.IdVehicleCategory,
                Brand = seed.Brand,
                Model = seed.Model,
                Year = seed.Year,
                CurrentMileage = seed.CurrentMileage,
                LastPreventiveMaintenanceMileage = seed.LastPreventive,
                Status = seed.Status,
                CreatedAt = DateTime.UtcNow,
                DeactivationDate = seed.Status == VehicleStatus.DadoDeBaja ? DateTime.UtcNow : null
            });
        }

        context.SaveChanges();

        // Cada repuesto tiene ofertas de dos o tres proveedores.
        var linkSeeds = new (string SpareCode, string[] Suppliers, decimal Price, int Days)[]
        {
            ("REP-001", new[] { "Repuestos del Norte S.A.", "AutoPartes Central", "Filtros Globales" },    8500m,   3),
            ("REP-002", new[] { "Frenos y Clutches Maxi", "AutoPartes Central" },                          24500m,  5),
            ("REP-003", new[] { "Neumáticos Andina", "Repuestos del Norte S.A." },                          98000m,  7),
            ("REP-004", new[] { "Baterías Power", "AutoPartes Central", "Accesorios Flota Total" },         62000m,  2),
            ("REP-005", new[] { "Motor y Transmisión Pro", "Repuestos del Norte S.A." },                   18750m,  10),
            ("REP-006", new[] { "Lubricantes Express", "Distribuidora Chasis" },                            4200m,   4),
            ("REP-007", new[] { "Lubricantes Express", "Distribuidora Chasis", "Repuestos del Norte S.A." }, 9900m,  3),
            ("REP-008", new[] { "Motor y Transmisión Pro", "AutoPartes Central" },                         41000m,  8),
            ("REP-009", new[] { "Motor y Transmisión Pro", "Baterías Power" },                             145000m, 12),
            ("REP-010", new[] { "Frenos y Clutches Maxi", "Distribuidora Chasis" },                         87000m,  9)
        };

        foreach (var seed in linkSeeds)
        {
            var sparePart = spareParts.FirstOrDefault(s => s.Code == seed.SpareCode);
            if (sparePart is null)
                continue;

            foreach (var supplierName in seed.Suppliers)
            {
                var supplier = suppliers.FirstOrDefault(s => s.Name == supplierName);
                if (supplier is null)
                    continue;

                if (!links.Any(ps => ps.IdSparePart == sparePart.IdSparePart && ps.IdSupplier == supplier.IdSupplier))
                    links.Add(new SparePartSupplier
                    {
                        IdSparePart = sparePart.IdSparePart,
                        IdSupplier = supplier.IdSupplier,
                        OfferedPrice = seed.Price,
                        EstimatedDeliveryDays = seed.Days
                    });
            }
        }
    }
}
