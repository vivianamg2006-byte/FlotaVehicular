using System.Text.Json.Serialization;
using FlotaVehicular.Application.Implementation;
using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Application.Mapper;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Data.Seed;
using FlotaVehicular.Infrastructure.Implementation;
using FlotaVehicular.Infrastructure.Interfaces;
using FlotaVehicular.WebAPI.Handlers;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

// Punto de partida de la API: aquí se registran los servicios y el pipeline de la respuesta.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Los errores de validación usan el mismo formato que las demás respuestas.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Los enums se envían como texto y las propiedades se serializan en camelCase.
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Los errores de validación se devuelven con el mismo formato Result<T>.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value!.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors.Select(error => error.ErrorMessage).ToArray());

            var result = Result<Dictionary<string, string[]>>.Failure(400, "Bad Request",
                "Uno o más datos no son válidos.");
            result.Data = errors;

            return new BadRequestObjectResult(result);
        };
    });

// Documentación OpenAPI de los endpoints de la API.
builder.Services.AddOpenApi();

// Esta conexión apunta a la base de datos de la flota.
var connectionString = builder.Configuration.GetConnectionString("FlotaVehicular")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'FlotaVehicular'. Configúrela en appsettings, user-secrets o la variable ConnectionStrings__FlotaVehicular.");

// EF Core con SQL Server, y la semilla que se ejecuta sobre la base de datos.
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(connectionString)

    .UseSeeding((context, _) => FleetSeeder.Seed(context))

    .UseAsyncSeeding(async (context, _, ct) => await FleetSeeder.SeedAsync(context, ct)));

// Registrar los mapeos entre entidades y DTO.
MapsterConfig.RegisterMaps();

var mapsterConfig = TypeAdapterConfig.GlobalSettings;

builder.Services.AddSingleton(mapsterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();
builder.Services.AddMapster();

// Repositorios y servicios de los módulos.
builder.Services.AddScoped<IVehicleCategoryRepository, VehicleCategoryRepository>();
builder.Services.AddScoped<IVehicleCategoryService, VehicleCategoryService>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IVehicleImageRepository, VehicleImageRepository>();
builder.Services.AddScoped<IVehicleImageService, VehicleImageService>();
builder.Services.AddScoped<ISparePartRepository, SparePartRepository>();
builder.Services.AddScoped<ISparePartService, SparePartService>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ISparePartSupplierRepository, SparePartSupplierRepository>();
builder.Services.AddScoped<ISparePartSupplierService, SparePartSupplierService>();
builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();
builder.Services.AddScoped<ITechnicianService, TechnicianService>();
builder.Services.AddScoped<ITechnicianSpecialtyRepository, TechnicianSpecialtyRepository>();
builder.Services.AddScoped<ITechnicianSpecialtyService, TechnicianSpecialtyService>();
builder.Services.AddScoped<IMaintenanceOrderRepository, MaintenanceOrderRepository>();
builder.Services.AddScoped<IMaintenanceOrderService, MaintenanceOrderService>();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Las imágenes se guardan en disco y su referencia queda en la base de datos.
var imagesRelativePath = builder.Configuration["Storage:VehicleImagesPath"] ?? "wwwroot/uploads/vehicles";
var publicPrefix = imagesRelativePath
    .Replace('\\', '/')
    .Trim('/')
    .Replace("wwwroot/", string.Empty);

builder.Services.AddSingleton<IImageStorage>(_ =>
    new LocalImageStorage(builder.Environment.ContentRootPath, imagesRelativePath, publicPrefix));

// Se construye la aplicación y se registra el manejo de errores HTTP.
var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    var statusCode = httpContext.Response.StatusCode;
    string title = statusCode switch
    {
        404 => "Not Found",
        405 => "Method Not Allowed",
        _ => "Error"
    };
    string detail = statusCode switch
    {
        404 => "Recurso no encontrado.",
        405 => "Método no permitido.",
        _ => "Solicitud no válida."
    };

    var result = FlotaVehicular.Domain.DTO.PatternResult.Result<object>.Failure(statusCode, title, detail);
    httpContext.Response.ContentType = "application/json; charset=utf-8";
    await httpContext.Response.WriteAsJsonAsync(result);
});

// Scalar queda disponible al ejecutar en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Las migraciones se aplican por separado, no al arrancar la API.
app.Run();
