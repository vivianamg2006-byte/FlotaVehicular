using System.Security.Claims;
using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.WebAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlotaVehicular.WebAPI.Controllers;

// Endpoints de órdenes de mantenimiento.
[Authorize]
[Route("api/maintenanceorder")]
[ApiController]
public class MaintenanceOrderController : ControllerBase
{
    private const string InvalidSessionMessage = "La sesión ya no es válida. Inicie sesión nuevamente.";

    private readonly IMaintenanceOrderService _service;

    public MaintenanceOrderController(IMaintenanceOrderService service)
    {
        _service = service;
    }

    // Lista las órdenes que cumplen el filtro indicado.
    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] MaintenanceOrderQueryDto query)
    {
        if (!TryGetUserId(out var idUser))
            return InvalidSession<IEnumerable<MaintenanceOrderListDto>>();

        // El Solicitante nunca puede consultar órdenes ajenas.
        if (User.IsInRole(AppRoles.Requester))
            query.CreatedByUserId = idUser;

        var result = await _service.GetAllAsync(query);
        return StatusCode(result.Status, result);
    }

    // Lista órdenes con paginación y filtros.
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedAsync([FromQuery] MaintenanceOrderQueryDto query)
    {
        if (!TryGetUserId(out var idUser))
            return InvalidSession<IEnumerable<MaintenanceOrderListDto>>();

        if (User.IsInRole(AppRoles.Requester))
            query.CreatedByUserId = idUser;

        var result = await _service.GetPagedAsync(query);
        return StatusCode(result.Status, result);
    }

    // Devuelve el detalle de una orden de mantenimiento.
    [HttpGet("{id:int}", Name = "GetMaintenanceOrderById")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        if (!TryGetUserId(out var idUser))
            return InvalidSession<MaintenanceOrderDetailDto>();

        var result = await _service.GetByIdAsync(id);

        // Para el Solicitante, una orden ajena es indistinguible de una inexistente.
        if (result.IsSuccess && User.IsInRole(AppRoles.Requester)
            && result.Data!.CreatedByUserId != idUser)
        {
            return StatusCode(404, Result<MaintenanceOrderDetailDto>.Failure(404, "Not Found",
                $"La orden {id} no existe."));
        }

        return StatusCode(result.Status, result);
    }

    // Cambia el estado respetando el ciclo de vida de la orden.
    [Authorize(Roles = AppRoles.AdminOrCoordinator)]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> ChangeStatusAsync(
        [FromRoute] int id, [FromBody] MaintenanceOrderStatusDto status)
    {
        var result = await _service.ChangeStatusAsync(id, status);
        return StatusCode(result.Status, result);
    }

    // Crea una orden a nombre del usuario autenticado.
    [Authorize(Roles = AppRoles.AdminOrRequester)]
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] MaintenanceOrderCreateDto order)
    {
        if (!TryGetUserId(out var idUser))
            return InvalidSession<MaintenanceOrderDetailDto>();

        var result = await _service.AddAsync(order, idUser);
        if (!result.IsSuccess)
            return StatusCode(result.Status, result);

        return CreatedAtRoute("GetMaintenanceOrderById", new { id = result.Data!.IdMaintenanceOrder }, result);
    }

    // Lee el identificador del claim enviado en el token.
    private bool TryGetUserId(out int idUser)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out idUser) && idUser > 0;
    }

    private IActionResult InvalidSession<T>()
        => StatusCode(401, Result<T>.Failure(401, "Unauthorized", InvalidSessionMessage));
}
