using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

public interface IVehicleService
{
    Task<Result<IEnumerable<VehicleListDto>>> GetAllAsync(VehicleQueryDto query);
    Task<Result<IEnumerable<VehicleListDto>>> GetPagedAsync(VehicleQueryDto query);
    Task<Result<VehicleDetailDto>> GetByIdAsync(int id);
    Task<Result<VehicleDetailDto>> AddAsync(VehicleCreateDto vehicle);
    Task<Result<VehicleDetailDto>> UpdateAsync(int id, VehicleUpdateDto vehicle);
    Task<Result<VehicleDetailDto>> ChangeStatusAsync(int id, VehicleStatusDto status);

    Task<Result<int>> DeactivateAsync(int id);
}
