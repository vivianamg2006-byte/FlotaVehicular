using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FlotaVehicular.Application.Implementation;
using FlotaVehicular.Application.Mapper;
using FlotaVehicular.Domain.DTO;
using FlotaVehicular.Domain.Entities;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Implementation;
using FlotaVehicular.Infrastructure.Interfaces;
using FlotaVehicular.WebAPI.Controllers;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace EstadosOrden.Tests;

public class OrderStatusTests
{
    static OrderStatusTests() => MapsterConfig.RegisterMaps();

    private static MaintenanceOrderService Service(FakeRepository repository)
        => new(repository, new Mapper(TypeAdapterConfig.GlobalSettings));

    private static MaintenanceOrder Order(OrderStatus status) => new()
    {
        IdMaintenanceOrder = 1, OrderCode = "OM-TEST-0001", IdVehicle = 1,
        MaintenanceType = MaintenanceType.Preventivo, Reason = "Revisión de prueba",
        Priority = OrderPriority.Media, TotalCost = 25000m, Status = status,
        RequestDate = new DateTime(2026, 10, 1), CreatedAt = new DateTime(2026, 10, 1),
        ScheduledDate = new DateTime(2026, 10, 30),
        ClosedDate = status is OrderStatus.Completada or OrderStatus.Cancelada or OrderStatus.Rechazada
            ? new DateTime(2026, 10, 2, 10, 0, 0, DateTimeKind.Utc) : null,
        Vehicle = new Vehicle
        {
            IdVehicle = 1, Code = "VEH-TEST", LicensePlate = "TEST001", Name = "Vehículo prueba",
            Brand = "Toyota", Model = "Hilux", Year = 2024, Status = VehicleStatus.Operativo,
            CurrentMileage = 25000, LastPreventiveMaintenanceMileage = 20000,
            AcquisitionDate = new DateTime(2024, 1, 1), CreatedAt = new DateTime(2024, 1, 1),
            Category = new VehicleCategory { Name = "Prueba", PreventiveMaintenanceIntervalKm = 5000 }
        }
    };

    // Matriz completa: acepta solo los movimientos definidos, sin saltos ni reaperturas.
    public static IEnumerable<object[]> AllTransitions()
    {
        var allowed = new HashSet<(OrderStatus, OrderStatus)>
        {
            (OrderStatus.Solicitada, OrderStatus.Diagnosticada),
            (OrderStatus.Solicitada, OrderStatus.Cancelada),
            (OrderStatus.Diagnosticada, OrderStatus.Aprobada),
            (OrderStatus.Diagnosticada, OrderStatus.Rechazada),
            (OrderStatus.Diagnosticada, OrderStatus.Cancelada),
            (OrderStatus.Aprobada, OrderStatus.EnEjecucion),
            (OrderStatus.Aprobada, OrderStatus.Cancelada),
            (OrderStatus.EnEjecucion, OrderStatus.EnEsperaDeRepuesto),
            (OrderStatus.EnEjecucion, OrderStatus.Completada),
            (OrderStatus.EnEjecucion, OrderStatus.Cancelada),
            (OrderStatus.EnEsperaDeRepuesto, OrderStatus.EnEjecucion),
            (OrderStatus.EnEsperaDeRepuesto, OrderStatus.Cancelada)
        };
        foreach (var from in Enum.GetValues<OrderStatus>())
        foreach (var to in Enum.GetValues<OrderStatus>())
            yield return new object[] { from, to, from == to || allowed.Contains((from, to)) };
    }

    [Theory]
    [MemberData(nameof(AllTransitions))]
    public async Task Transition_matrix_preserves_unrelated_data_and_closure(
        OrderStatus from, OrderStatus to, bool allowed)
    {
        var repository = new FakeRepository { Current = Order(from) };
        var previousClose = repository.Current.ClosedDate;
        var before = DateTime.UtcNow;
        var result = await Service(repository).ChangeStatusAsync(1, new() { Status = to });

        Assert.Equal(allowed ? 200 : 409, result.Status);
        Assert.Equal(allowed, result.IsSuccess);
        Assert.Equal(allowed ? to : from, repository.Current.Status);
        Assert.Equal(allowed && from != to ? 1 : 0, repository.UpdateCalls);
        Assert.Equal(25000m, repository.Current.TotalCost);
        Assert.Equal("Revisión de prueba", repository.Current.Reason);
        Assert.Equal(new DateTime(2026, 10, 30), repository.Current.ScheduledDate);
        Assert.Equal(VehicleStatus.Operativo, repository.Current.Vehicle.Status);
        Assert.Equal(25000, repository.Current.Vehicle.CurrentMileage);
        if (allowed && from != to && to is OrderStatus.Completada or OrderStatus.Rechazada or OrderStatus.Cancelada)
            Assert.InRange(repository.Current.ClosedDate!.Value, before, DateTime.UtcNow);
        else
            Assert.Equal(previousClose, repository.Current.ClosedDate);
        if (allowed)
        {
            Assert.NotNull(result.Data);
            Assert.Equal(to.ToString(), result.Data.Status);
            Assert.Equal(repository.Current.ClosedDate, result.Data.ClosedDate);
        }
    }

    [Fact]
    public async Task Complete_lifecycle_with_waiting_and_retry_keeps_the_original_close_date()
    {
        var repository = new FakeRepository { Current = Order(OrderStatus.Solicitada) };
        var service = Service(repository);
        foreach (var state in new[] { OrderStatus.Diagnosticada, OrderStatus.Aprobada,
                     OrderStatus.EnEjecucion, OrderStatus.EnEsperaDeRepuesto,
                     OrderStatus.EnEjecucion, OrderStatus.Completada })
            Assert.True((await service.ChangeStatusAsync(1, new() { Status = state })).IsSuccess);
        var closed = repository.Current.ClosedDate;
        Assert.NotNull(closed);
        Assert.Equal(200, (await service.ChangeStatusAsync(1, new() { Status = OrderStatus.Completada })).Status);
        Assert.Equal(closed, repository.Current.ClosedDate);
        Assert.Equal(6, repository.UpdateCalls);
    }

    [Fact]
    public async Task Bad_input_missing_order_and_concurrent_change_do_not_save()
    {
        var repository = new FakeRepository { Current = Order(OrderStatus.Solicitada) };
        var service = Service(repository);
        Assert.Equal(400, (await service.ChangeStatusAsync(0, new() { Status = OrderStatus.Diagnosticada })).Status);
        Assert.Equal(400, (await service.ChangeStatusAsync(1, new())).Status);
        Assert.Equal(400, (await service.ChangeStatusAsync(1, new() { Status = (OrderStatus)999 })).Status);
        Assert.Equal(404, (await service.ChangeStatusAsync(999, new() { Status = OrderStatus.Diagnosticada })).Status);
        Assert.Equal(0, repository.UpdateCalls);
        repository.RejectUpdate = true;
        Assert.Equal(409, (await service.ChangeStatusAsync(1, new() { Status = OrderStatus.Diagnosticada })).Status);
        Assert.Equal(OrderStatus.Solicitada, repository.Current.Status);
        Assert.Null(repository.Current.ClosedDate);
    }

    [Fact]
    public async Task Relational_update_is_persisted_and_a_stale_writer_cannot_overwrite_it()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var entity = Order(OrderStatus.EnEjecucion);
        db.MaintenanceOrder.Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repository = new MaintenanceOrderRepository(db);
        var close = DateTime.UtcNow;
        Assert.True(await repository.TryChangeStatusAsync(1, OrderStatus.EnEjecucion, OrderStatus.Completada, close));
        Assert.False(await repository.TryChangeStatusAsync(1, OrderStatus.EnEjecucion, OrderStatus.Cancelada, close.AddHours(1)));
        Assert.False(await repository.TryChangeStatusAsync(999, OrderStatus.Solicitada, OrderStatus.Cancelada, close));
        var saved = await repository.GetByIdAsync(1);
        Assert.NotNull(saved);
        Assert.Equal(OrderStatus.Completada, saved.Status);
        Assert.Equal(close, saved.ClosedDate);
        Assert.Equal(25000m, saved.TotalCost);
        Assert.Equal(VehicleStatus.Operativo, saved.Vehicle.Status);
    }

    [Fact]
    public async Task Actual_http_pipeline_validates_json_exposes_route_and_returns_detail()
    {
        using var factory = new ApiFactory();
        using var client = factory.CreateClient();
        foreach (var json in new[] { "{}", "null", "{\"status\":null}", "{\"status\":999}",
                     "{\"status\":\"Desconocido\"}", "{\"status\":\"999\"}", "{" })
        {
            using var bad = await client.PutAsync("/api/maintenanceorder/1/status",
                new StringContent(json, Encoding.UTF8, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
            var error = await bad.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(error.GetProperty("isSuccess").GetBoolean());
        }
        using var missing = await client.PutAsJsonAsync("/api/maintenanceorder/999/status", new { status = "Diagnosticada" });
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var invalid = await client.PutAsJsonAsync("/api/maintenanceorder/1/status", new { status = "Completada" });
        Assert.Equal(HttpStatusCode.Conflict, invalid.StatusCode);
        using var success = await client.PutAsJsonAsync("/api/maintenanceorder/1/status", new { status = "Diagnosticada" });
        Assert.Equal(HttpStatusCode.OK, success.StatusCode);
        var detail = await success.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(detail.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Diagnosticada", detail.GetProperty("data").GetProperty("status").GetString());
        using var retry = await client.PutAsJsonAsync("/api/maintenanceorder/1/status", new { status = "Diagnosticada" });
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        using var get = await client.GetAsync("/api/maintenanceorder/1");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var document = await client.GetFromJsonAsync<JsonElement>("/openapi/v1.json");
        Assert.True(document.GetProperty("paths").GetProperty("/api/maintenanceorder/{id}/status").TryGetProperty("put", out _));
    }

    private sealed class ApiFactory : WebApplicationFactory<MaintenanceOrderController>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseContentRoot(Path.GetFullPath("../../../../../FlotaVehicular.WebAPI", AppContext.BaseDirectory));
            builder.UseSetting("ConnectionStrings:FlotaVehicular", "Server=unused;Database=unused;Integrated Security=true");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IMaintenanceOrderRepository>();
                services.AddSingleton<IMaintenanceOrderRepository>(new FakeRepository { Current = Order(OrderStatus.Solicitada) });
            });
        }
    }

    private sealed class FakeRepository : IMaintenanceOrderRepository
    {
        public required MaintenanceOrder Current { get; init; }
        public int UpdateCalls { get; private set; }
        public bool RejectUpdate { get; set; }
        public Task<MaintenanceOrder?> GetByIdAsync(int id) => Task.FromResult(id == Current.IdMaintenanceOrder ? Current : null);
        public Task<bool> TryChangeStatusAsync(int id, OrderStatus expectedStatus, OrderStatus newStatus, DateTime? closedDate)
        {
            UpdateCalls++;
            if (RejectUpdate || id != Current.IdMaintenanceOrder || Current.Status != expectedStatus)
                return Task.FromResult(false);
            Current.Status = newStatus;
            Current.ClosedDate = closedDate;
            return Task.FromResult(true);
        }
        public Task<IEnumerable<MaintenanceOrder>> GetPagedAsync(MaintenanceOrderQueryDto query) => throw new NotSupportedException();
        public Task<IEnumerable<MaintenanceOrder>> GetAllAsync(MaintenanceOrderQueryDto query) => throw new NotSupportedException();
        public Task<int> CountAsync(MaintenanceOrderQueryDto query) => throw new NotSupportedException();
        public Task<Vehicle?> GetVehicleAsync(int idVehicle) => throw new NotSupportedException();
        public Task<Specialty?> GetSpecialtyAsync(int idSpecialty) => throw new NotSupportedException();
        public Task<int> CountByDateAsync(DateTime date) => throw new NotSupportedException();
        public Task<bool> ExistsOrderCodeAsync(string orderCode) => throw new NotSupportedException();
        public Task AddAsync(MaintenanceOrder order) => throw new NotSupportedException();
        public Task SaveChangesAsync() => throw new NotSupportedException();
    }
}
