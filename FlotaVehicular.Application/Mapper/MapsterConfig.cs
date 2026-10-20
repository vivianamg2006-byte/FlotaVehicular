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

        // Especialidades
        TypeAdapterConfig<Specialty, SpecialtyListDto>
            .NewConfig()
            .Map(dest => dest.TechnicianCount, src => src.TechnicianSpecialties.Count);

        // El detalle incluye los técnicos certificados en la especialidad.
        TypeAdapterConfig<Specialty, SpecialtyDetailDto>
            .NewConfig()
            .Map(dest => dest.TechnicianCount, src => src.TechnicianSpecialties.Count)
            .Map(dest => dest.Technicians, src => src.TechnicianSpecialties
                .Select(ts => new TechnicianSpecialtyListDto
                {
                    IdTechnician = ts.IdTechnician,
                    TechnicianName = ts.Technician.FullName,
                    IdentificationNumber = ts.Technician.IdentificationNumber,
                    TechnicianActive = ts.Technician.IsActive,
                    IdSpecialty = ts.IdSpecialty,
                    SpecialtyName = src.Name,
                    CertificationLevel = ts.CertificationLevel,
                    SinceDate = ts.SinceDate
                })
                .OrderBy(x => x.TechnicianName)
                .ToList());

        TypeAdapterConfig<SpecialtyCreateDto, Specialty>
            .NewConfig()
            .Ignore(dest => dest.IdSpecialty)
            .Ignore(dest => dest.TechnicianSpecialties)
            .Ignore(dest => dest.MaintenanceOrders);

        TypeAdapterConfig<SpecialtyUpdateDto, Specialty>
            .NewConfig()
            .Ignore(dest => dest.IdSpecialty)
            .Ignore(dest => dest.TechnicianSpecialties)
            .Ignore(dest => dest.MaintenanceOrders);

        // Relación entre técnicos y especialidades
        TypeAdapterConfig<TechnicianSpecialty, TechnicianSpecialtyListDto>
            .NewConfig()
            .Map(dest => dest.TechnicianName, src => src.Technician.FullName)
            .Map(dest => dest.IdentificationNumber, src => src.Technician.IdentificationNumber)
            .Map(dest => dest.TechnicianActive, src => src.Technician.IsActive)
            .Map(dest => dest.SpecialtyName, src => src.Specialty.Name);

        TypeAdapterConfig<TechnicianSpecialtyCreateDto, TechnicianSpecialty>
            .NewConfig()
            .Ignore(dest => dest.Technician)
            .Ignore(dest => dest.Specialty);

        // Técnicos
        TypeAdapterConfig<Technician, TechnicianListDto>
            .NewConfig()
            .Map(dest => dest.SpecialtyCount, src => src.TechnicianSpecialties.Count);

        // El detalle incluye las especialidades certificadas del técnico.
        TypeAdapterConfig<Technician, TechnicianDetailDto>
            .NewConfig()
            .Map(dest => dest.SpecialtyCount, src => src.TechnicianSpecialties.Count)
            .Map(dest => dest.Specialties, src => src.TechnicianSpecialties
                .Select(ts => new TechnicianSpecialtyListDto
                {
                    IdTechnician = ts.IdTechnician,
                    TechnicianName = src.FullName,
                    IdentificationNumber = src.IdentificationNumber,
                    TechnicianActive = src.IsActive,
                    IdSpecialty = ts.IdSpecialty,
                    SpecialtyName = ts.Specialty.Name,
                    CertificationLevel = ts.CertificationLevel,
                    SinceDate = ts.SinceDate
                })
                .OrderBy(x => x.SpecialtyName)
                .ToList());

        TypeAdapterConfig<TechnicianCreateDto, Technician>
            .NewConfig()
            .Ignore(dest => dest.IdTechnician)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.TechnicianSpecialties);

        TypeAdapterConfig<TechnicianUpdateDto, Technician>
            .NewConfig()
            .Ignore(dest => dest.IdTechnician)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.TechnicianSpecialties);

        // Órdenes de mantenimiento
        TypeAdapterConfig<MaintenanceOrder, MaintenanceOrderListDto>
            .NewConfig()
            .Map(dest => dest.VehicleCode, src => src.Vehicle.Code)
            .Map(dest => dest.VehicleLicensePlate, src => src.Vehicle.LicensePlate)
            .Map(dest => dest.MaintenanceType, src => src.MaintenanceType.ToString())
            .Map(dest => dest.Priority, src => src.Priority.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.RequiredSpecialtyName,
                src => src.RequiredSpecialty != null ? src.RequiredSpecialty.Name : null);

        TypeAdapterConfig<MaintenanceOrder, MaintenanceOrderDetailDto>
            .NewConfig()
            .Map(dest => dest.VehicleCode, src => src.Vehicle.Code)
            .Map(dest => dest.VehicleLicensePlate, src => src.Vehicle.LicensePlate)
            .Map(dest => dest.VehicleName, src => src.Vehicle.Name)
            .Map(dest => dest.VehicleStatus, src => src.Vehicle.Status.ToString())
            .Map(dest => dest.MaintenanceType, src => src.MaintenanceType.ToString())
            .Map(dest => dest.Priority, src => src.Priority.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.RequiredSpecialtyName,
                src => src.RequiredSpecialty != null ? src.RequiredSpecialty.Name : null);

        TypeAdapterConfig<MaintenanceOrderCreateDto, MaintenanceOrder>
            .NewConfig()
            .Ignore(dest => dest.IdMaintenanceOrder)
            .Ignore(dest => dest.OrderCode)
            .Ignore(dest => dest.RequestDate)
            .Ignore(dest => dest.ClosedDate)
            .Ignore(dest => dest.TotalCost)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.Vehicle)
            .Ignore(dest => dest.RequiredSpecialty);
    }
}
