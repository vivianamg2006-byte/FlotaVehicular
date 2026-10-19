using System.Text.Json.Serialization;
using FlotaVehicular.Application.Implementation;
using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Application.Mapper;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Data.Seed;
using FlotaVehicular.Infrastructure.Implementation;
using FlotaVehicular.Infrastructure.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Los errores de validación usan el mismo formato que las demás respuestas.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
    })
    .ConfigureApiBehaviorOptions(options =>
    {
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

builder.Services.AddOpenApi();

// Esta conexión apunta a la base de FlotaVehicular.
var connectionString = builder.Configuration.GetConnectionString("FlotaVehicular")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'FlotaVehicular'. Configúrela en appsettings, user-secrets o la variable ConnectionStrings__FlotaVehicular.");

// La semilla se ejecuta al aplicar las migraciones.
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

// Las imágenes se guardan en disco y su referencia queda en la base de datos.
var imagesRelativePath = builder.Configuration["Storage:VehicleImagesPath"] ?? "wwwroot/uploads/vehicles";
var publicPrefix = imagesRelativePath
    .Replace('\\', '/')
    .Trim('/')
    .Replace("wwwroot/", string.Empty);

builder.Services.AddSingleton<IImageStorage>(_ =>
    new LocalImageStorage(builder.Environment.ContentRootPath, imagesRelativePath, publicPrefix));

var app = builder.Build();

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
