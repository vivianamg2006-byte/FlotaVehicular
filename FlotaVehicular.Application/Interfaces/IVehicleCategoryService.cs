using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

// Casos de uso de la gestión de categorías de vehículo.
public interface IVehicleCategoryService
{
    Task<Result<IEnumerable<VehicleCategoryListDto>>> GetAllAsync(string? search);
    Task<Result<IEnumerable<VehicleCategoryListDto>>> GetPagedAsync(VehicleCategoryQueryDto query);
    Task<Result<VehicleCategoryDetailDto>> GetByIdAsync(int id);
    Task<Result<VehicleCategoryDetailDto>> AddAsync(VehicleCategoryCreateDto category);
    Task<Result<VehicleCategoryDetailDto>> UpdateAsync(int id, VehicleCategoryUpdateDto category);
    Task<Result<int>> DeleteAsync(int id);
}
