using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;

namespace FlotaVehicular.Application.Interfaces;

// Casos de uso de la gestión de órdenes de mantenimiento.
public interface IMaintenanceOrderService
{
    Task<Result<IEnumerable<MaintenanceOrderListDto>>> GetPagedAsync(MaintenanceOrderQueryDto query);
    Task<Result<IEnumerable<MaintenanceOrderListDto>>> GetAllAsync(MaintenanceOrderQueryDto query);
    Task<Result<MaintenanceOrderDetailDto>> GetByIdAsync(int id);
    Task<Result<MaintenanceOrderDetailDto>> AddAsync(MaintenanceOrderCreateDto order, int createdByUserId);
    Task<Result<MaintenanceOrderDetailDto>> ChangeStatusAsync(int id, MaintenanceOrderStatusDto status);
}
