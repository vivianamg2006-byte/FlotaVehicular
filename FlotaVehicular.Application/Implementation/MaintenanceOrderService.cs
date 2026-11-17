using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Interfaces;
using MapsterMapper;

namespace FlotaVehicular.Application.Implementation;

// Casos de uso de la gestión de órdenes de mantenimiento.
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

    // Lista órdenes con paginación y filtros.
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

    // Lista las órdenes que cumplen el filtro indicado.
    public async Task<Result<IEnumerable<MaintenanceOrderListDto>>> GetAllAsync(MaintenanceOrderQueryDto query)
    {
        var orders = await _repository.GetAllAsync(query);

        var data = _mapper.Map<IEnumerable<MaintenanceOrderListDto>>(orders);

        return Result<IEnumerable<MaintenanceOrderListDto>>.Success(data);
    }

    // Devuelve el detalle de una orden de mantenimiento.
    public async Task<Result<MaintenanceOrderDetailDto>> GetByIdAsync(int id)
    {
        var order = await _repository.GetByIdAsync(id);

        if (order is null)
            return Result<MaintenanceOrderDetailDto>.Failure(404, "Not Found", $"La orden {id} no existe.");

        return Result<MaintenanceOrderDetailDto>.Success(_mapper.Map<MaintenanceOrderDetailDto>(order));
    }

    // Crea una orden de mantenimiento.
    public async Task<Result<MaintenanceOrderDetailDto>> AddAsync(
        MaintenanceOrderCreateDto order, int createdByUserId)
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

        if (order.ScheduledDate.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            if (order.ScheduledDate.Value.Date < today)
                return Result<MaintenanceOrderDetailDto>.Failure(400, "Bad Request",
                    "La fecha programada no puede ser anterior a hoy.");
        }

        var now = DateTime.UtcNow;

        var entity = _mapper.Map<MaintenanceOrder>(order);
        entity.OrderCode = await GenerateOrderCodeAsync(now);
        entity.MaintenanceType = maintenanceType;
        entity.Priority = order.Priority!.Value;
        entity.Reason = order.Reason!.Trim();
        entity.RequestDate = now;
        entity.CreatedAt = now;

        // La identidad del creador la impone el token, nunca el cuerpo de la petición.
        entity.CreatedByUserId = createdByUserId;

        // Toda orden nace en estado Solicitada.
        entity.Status = OrderStatus.Solicitada;
        entity.TotalCost = 0m;

        await _repository.AddAsync(entity);

        var created = await _repository.GetByIdAsync(entity.IdMaintenanceOrder);
        var data = _mapper.Map<MaintenanceOrderDetailDto>(created!);

        return Result<MaintenanceOrderDetailDto>.Success(data, 201, "Created",
            $"Orden {entity.OrderCode} creada.");
    }

    // Actualiza únicamente el estado de la orden y su fecha de cierre.
    public async Task<Result<MaintenanceOrderDetailDto>> ChangeStatusAsync(int id, MaintenanceOrderStatusDto status)
    {
        if (id < 1)
            return Result<MaintenanceOrderDetailDto>.Failure(400, "Bad Request",
                "El identificador de la orden debe ser mayor que cero.");

        if (status.Status is null || !Enum.IsDefined(status.Status.Value))
            return Result<MaintenanceOrderDetailDto>.Failure(400, "Bad Request",
                "Indique un estado de orden válido.");

        var order = await _repository.GetByIdAsync(id);
        if (order is null)
            return Result<MaintenanceOrderDetailDto>.Failure(404, "Not Found", $"La orden {id} no existe.");

        var newStatus = status.Status.Value;
        if (order.Status == newStatus)
            return Result<MaintenanceOrderDetailDto>.Success(
                _mapper.Map<MaintenanceOrderDetailDto>(order), 200, "OK",
                "La orden ya tiene el estado indicado.");

        if (!CanTransition(order.Status, newStatus))
            return Result<MaintenanceOrderDetailDto>.Failure(409, "Conflict",
                $"No se permite cambiar la orden de {order.Status} a {newStatus}.");

        DateTime? closedDate = newStatus is OrderStatus.Completada or OrderStatus.Rechazada or OrderStatus.Cancelada
            ? DateTime.UtcNow
            : null;

        if (!await _repository.TryChangeStatusAsync(id, order.Status, newStatus, closedDate))
            return Result<MaintenanceOrderDetailDto>.Failure(409, "Conflict",
                "La orden fue modificada por otra solicitud. Consulte su estado e intente nuevamente.");

        var updated = await _repository.GetByIdAsync(id);
        if (updated is null)
            return Result<MaintenanceOrderDetailDto>.Failure(404, "Not Found", $"La orden {id} no existe.");

        return Result<MaintenanceOrderDetailDto>.Success(
            _mapper.Map<MaintenanceOrderDetailDto>(updated), 200, "OK", "Estado de la orden actualizado.");
    }

    private static bool CanTransition(OrderStatus current, OrderStatus next) => current switch
    {
        OrderStatus.Solicitada => next is OrderStatus.Diagnosticada or OrderStatus.Cancelada,
        OrderStatus.Diagnosticada => next is OrderStatus.Aprobada or OrderStatus.Rechazada or OrderStatus.Cancelada,
        OrderStatus.Aprobada => next is OrderStatus.EnEjecucion or OrderStatus.Cancelada,
        OrderStatus.EnEjecucion => next is OrderStatus.EnEsperaDeRepuesto or OrderStatus.Completada or OrderStatus.Cancelada,
        OrderStatus.EnEsperaDeRepuesto => next is OrderStatus.EnEjecucion or OrderStatus.Cancelada,
        _ => false
    };

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
