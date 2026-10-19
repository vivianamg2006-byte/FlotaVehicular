using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using Mapster;

namespace FlotaVehicular.Application.Mapper;

public static class MapsterConfig
{
    public static void RegisterMaps()
    {
        // Categorías
        TypeAdapterConfig<VehicleCategory, VehicleCategoryListDto>
            .NewConfig()
            .Map(dest => dest.VehicleCount, src => src.Vehicles.Count);

        TypeAdapterConfig<VehicleCategory, VehicleCategoryDetailDto>
            .NewConfig()
            .Map(dest => dest.VehicleCount, src => src.Vehicles.Count)
            .Map(dest => dest.VehicleCodes, src => src.Vehicles.Select(v => v.Code).OrderBy(c => c).ToList());

        TypeAdapterConfig<VehicleCategoryCreateDto, VehicleCategory>
            .NewConfig()
            .Ignore(dest => dest.IdVehicleCategory)
            .Ignore(dest => dest.Vehicles);

        TypeAdapterConfig<VehicleCategoryUpdateDto, VehicleCategory>
            .NewConfig()
            .Ignore(dest => dest.IdVehicleCategory)
            .Ignore(dest => dest.Vehicles);

        // Vehículos
        TypeAdapterConfig<Vehicle, VehicleListDto>
            .NewConfig()
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.StatusName, src => src.Status.ToString())

            .Map(dest => dest.KmToNextPreventiveMaintenance,
                src => src.Category.PreventiveMaintenanceIntervalKm - (src.CurrentMileage - src.LastPreventiveMaintenanceMileage));

        TypeAdapterConfig<Vehicle, VehicleDetailDto>
            .NewConfig()
            .Map(dest => dest.StatusName, src => src.Status.ToString())
            .Map(dest => dest.KmToNextPreventiveMaintenance,
                src => src.Category.PreventiveMaintenanceIntervalKm - (src.CurrentMileage - src.LastPreventiveMaintenanceMileage))
            .Map(dest => dest.ImageCount, src => src.Images.Count);

        TypeAdapterConfig<VehicleCreateDto, Vehicle>
            .NewConfig()
            .Ignore(dest => dest.IdVehicle)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.DeactivationDate)
            .Ignore(dest => dest.Category)
            .Ignore(dest => dest.Images)
            .Map(dest => dest.ImagePath, src => src.Image);

        // La imagen se conserva si la edición no envía una nueva referencia.
        TypeAdapterConfig<VehicleUpdateDto, Vehicle>
            .NewConfig()
            .Ignore(dest => dest.IdVehicle)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.DeactivationDate)
            .Ignore(dest => dest.Category)
            .Ignore(dest => dest.Images)
            .Ignore(dest => dest.ImagePath);

        // Imágenes
        TypeAdapterConfig<VehicleImage, VehicleImageDto>
            .NewConfig();

        // Repuestos
        TypeAdapterConfig<SparePart, SparePartListDto>
            .NewConfig()
            .Map(dest => dest.IsBelowMinimum, src => src.CurrentStock < src.MinStock)
            .Map(dest => dest.SupplierCount, src => src.SparePartSuppliers.Count);

        // El detalle incluye las condiciones de cada proveedor.
        TypeAdapterConfig<SparePart, SparePartDetailDto>
            .NewConfig()
            .Map(dest => dest.IsBelowMinimum, src => src.CurrentStock < src.MinStock)
            .Map(dest => dest.SupplierCount, src => src.SparePartSuppliers.Count)

            .Map(dest => dest.Suppliers, src => src.SparePartSuppliers
                .Select(ps => new SparePartSupplierListDto
                {
                    IdSparePart = ps.IdSparePart,
                    SparePartCode = src.Code,
                    SparePartName = src.Name,
                    IdSupplier = ps.IdSupplier,
                    SupplierName = ps.Supplier.Name,
                    SupplierActive = ps.Supplier.Status,
                    OfferedPrice = ps.OfferedPrice,
                    EstimatedDeliveryDays = ps.EstimatedDeliveryDays,
                    AssociatedAt = ps.AssociatedAt
                })
                .OrderBy(x => x.SupplierName)
                .ToList());

        TypeAdapterConfig<SparePartCreateDto, SparePart>
            .NewConfig()
            .Ignore(dest => dest.IdSparePart)
            .Ignore(dest => dest.SparePartSuppliers);

        TypeAdapterConfig<SparePartUpdateDto, SparePart>
            .NewConfig()
            .Ignore(dest => dest.IdSparePart)
            .Ignore(dest => dest.SparePartSuppliers);

        // Proveedores
        TypeAdapterConfig<Supplier, SupplierListDto>
            .NewConfig()
            .Map(dest => dest.SparePartCount, src => src.SparePartSuppliers.Count);

        TypeAdapterConfig<Supplier, SupplierDetailDto>
            .NewConfig()
            .Map(dest => dest.SparePartCount, src => src.SparePartSuppliers.Count)

            .Map(dest => dest.SpareParts, src => src.SparePartSuppliers
                .Select(ps => new SparePartSupplierListDto
                {
                    IdSparePart = ps.IdSparePart,
                    SparePartCode = ps.SparePart.Code,
                    SparePartName = ps.SparePart.Name,
                    IdSupplier = ps.IdSupplier,
                    SupplierName = src.Name,
                    SupplierActive = src.Status,
                    OfferedPrice = ps.OfferedPrice,
                    EstimatedDeliveryDays = ps.EstimatedDeliveryDays,
                    AssociatedAt = ps.AssociatedAt
                })
                .OrderBy(x => x.SparePartCode)
                .ToList());

        TypeAdapterConfig<SupplierCreateDto, Supplier>
            .NewConfig()
            .Ignore(dest => dest.IdSupplier)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.SparePartSuppliers);

        TypeAdapterConfig<SupplierUpdateDto, Supplier>
            .NewConfig()
            .Ignore(dest => dest.IdSupplier)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.SparePartSuppliers);

        // Relación entre repuestos y proveedores
        TypeAdapterConfig<SparePartSupplier, SparePartSupplierListDto>
            .NewConfig()
            .Map(dest => dest.SparePartCode, src => src.SparePart.Code)
            .Map(dest => dest.SparePartName, src => src.SparePart.Name)
            .Map(dest => dest.SupplierName, src => src.Supplier.Name)
            .Map(dest => dest.SupplierActive, src => src.Supplier.Status);

        TypeAdapterConfig<SparePartSupplierCreateDto, SparePartSupplier>
            .NewConfig()
            .Ignore(dest => dest.SparePart)
            .Ignore(dest => dest.Supplier)
            .Ignore(dest => dest.AssociatedAt);
    }
}
