using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

public interface IMaintenanceOrderService
{
    Task<Result<IEnumerable<MaintenanceOrderListDto>>> GetPagedAsync(MaintenanceOrderQueryDto query);
    Task<Result<MaintenanceOrderDetailDto>> GetByIdAsync(int id);
    Task<Result<MaintenanceOrderDetailDto>> AddAsync(MaintenanceOrderCreateDto order);
}
