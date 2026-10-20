using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

public class MaintenanceOrderService : IMaintenanceOrderService
{
    public const int MaxPageSize = 50;

    private readonly IMaintenanceOrderRepository _repository;
    private readonly IMapper _mapper;

    public MaintenanceOrderService(IMaintenanceOrderRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<MaintenanceOrderListDto>>> GetPagedAsync(MaintenanceOrderQueryDto query)
    {
        var pagingError = ValidatePaging(query.PageNumber, query.PageSize);
        if (pagingError is not null)
            return Result<IEnumerable<MaintenanceOrderListDto>>.Failure(400, "Bad Request", pagingError);

        var orders = await _repository.GetPagedAsync(query);
        var totalRecords = await _repository.CountAsync(query);

        var data = _mapper.Map<IEnumerable<MaintenanceOrderListDto>>(orders);

        return Result<IEnumerable<MaintenanceOrderListDto>>.Paged(data, query.PageNumber, query.PageSize, totalRecords);
    }

    public async Task<Result<MaintenanceOrderDetailDto>> GetByIdAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);

        if (order is null)
            return Result<MaintenanceOrderDetailDto>.Failure(404, "Not Found", $"La orden {id} no existe.");

        return Result<MaintenanceOrderDetailDto>.Success(_mapper.Map<MaintenanceOrderDetailDto>(order));
    }

    public async Task<Result<MaintenanceOrderDetailDto>> AddAsync(MaintenanceOrderCreateDto order)
    {
        var vehicle = await _repository.GetVehicleAsync(order.IdVehicle);

        if (vehicle is null)
            return Result<MaintenanceOrderDetailDto>.Failure(404, "Not Found",
                $"El vehículo {order.IdVehicle} no existe.");

        // Un vehículo de baja o fuera de servicio no puede recibir órdenes.
        if (vehicle.Status is VehicleStatus.DadoDeBaja or VehicleStatus.FueraDeServicio)
            return Result<MaintenanceOrderDetailDto>.Failure(400, "Bad Request",
                $"El vehículo {order.IdVehicle} está {vehicle.Status}; no se le pueden crear órdenes de mantenimiento.");

        var maintenanceType = order.MaintenanceType!.Value;

        // Un vehículo ya en mantenimiento solo admite una emergencia.
        if (vehicle.Status == VehicleStatus.EnMantenimiento && maintenanceType != MaintenanceType.Emergencia)
            return Result<MaintenanceOrderDetailDto>.Failure(400, "Bad Request",
                $"El vehículo {order.IdVehicle} ya está en mantenimiento; solo se permite una orden de tipo Emergencia.");

        if (order.IdRequiredSpecialty.HasValue
            && await _repository.GetSpecialtyAsync(order.IdRequiredSpecialty.Value) is null)
            return Result<MaintenanceOrderDetailDto>.Failure(404, "Not Found",
                $"La especialidad {order.IdRequiredSpecialty.Value} no existe.");

        var now = DateTime.UtcNow;

        var entity = _mapper.Map<MaintenanceOrder>(order);
        entity.OrderCode = await GenerateOrderCodeAsync(now);
        entity.MaintenanceType = maintenanceType;
        entity.Priority = order.Priority!.Value;
        entity.Reason = order.Reason!.Trim();
        entity.RequestDate = now;
        entity.CreatedAt = now;

        // Toda orden nace en estado Solicitada.
        entity.Status = OrderStatus.Solicitada;
        entity.TotalCost = 0m;

        await _repository.AddAsync(entity);

        var created = await _repository.GetByIdAsync(entity.IdMaintenanceOrder);
        var data = _mapper.Map<MaintenanceOrderDetailDto>(created!);

        return Result<MaintenanceOrderDetailDto>.Success(data, 201, "Created",
            $"Orden {entity.OrderCode} creada.");
    }

    // El código OM-yyyyMMdd-0001 es único y consecutivo por día.
    private async Task<string> GenerateOrderCodeAsync(DateTime date)
    {
        var prefix = $"OM-{date:yyyyMMdd}-";
        var sequence = await _repository.CountByDateAsync(date) + 1;

        var orderCode = $"{prefix}{sequence:0000}";

        while (await _repository.ExistsOrderCodeAsync(orderCode))
        {
            sequence++;
            orderCode = $"{prefix}{sequence:0000}";
        }

        return orderCode;
    }

    private static string? ValidatePaging(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return $"pageNumber debe ser 1 o mayor y pageSize debe estar entre 1 y {MaxPageSize}.";

        return null;
    }
}
