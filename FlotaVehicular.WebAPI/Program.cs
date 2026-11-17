using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using FlotaVehicular.Application.Implementation;
using FlotaVehicular.Application.Interfaces;
using FlotaVehicular.Application.Mapper;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Infrastructure.Data;
using FlotaVehicular.Infrastructure.Data.Seed;
using FlotaVehicular.Infrastructure.Implementation;
using FlotaVehicular.Infrastructure.Interfaces;
using FlotaVehicular.Infrastructure.Logger;
using FlotaVehicular.WebAPI.Filters;
using FlotaVehicular.WebAPI.Handlers;
using FlotaVehicular.WebAPI.Middleware;
using FlotaVehicular.WebAPI.Security;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

// Punto de partida de la API: aquí se registran los servicios y el pipeline de la respuesta.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Los errores de validación usan el mismo formato que las demás respuestas.
builder.Services.AddControllers(options =>
    {
        // Los Result de error quedan disponibles para la bitácora de peticiones.
        options.Filters.Add<ResultLoggingFilter>();
    })
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

// Configuración JWT: la ausencia de la sección o una clave corta impide arrancar la API.
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "Falta la sección 'Jwt'. Configúrela en appsettings, user-secrets o la variable Jwt__SecretKey.");

if (!JwtSettings.HasValidKeyLength(jwtSettings.SecretKey))
    throw new InvalidOperationException(
        "Jwt:SecretKey debe tener al menos 32 bytes. Configúrela en appsettings, user-secrets o la variable Jwt__SecretKey.");

// Autenticación Bearer con validación de emisor, audiencia, firma y vencimiento.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Los claims se consumen con los mismos tipos con los que se emitieron.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// CORS para los orígenes del frontend; si no se configuran, se usan los dos locales.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins is null || allowedOrigins.Length == 0)
    allowedOrigins = ["http://localhost:5173", "http://localhost:3000"];

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Esta conexión apunta a la base de datos de la flota.
var connectionString = builder.Configuration.GetConnectionString("FlotaVehicular")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión 'FlotaVehicular'. Configúrela en appsettings, user-secrets o la variable ConnectionStrings__FlotaVehicular.");

// El mismo hasher de la API genera los hashes de los usuarios iniciales.
var seedPasswordHasher = new Pbkdf2PasswordHasher();

// EF Core con SQL Server, y la semilla que se ejecuta sobre la base de datos.
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(connectionString)

    .UseSeeding((context, _) => FleetSeeder.Seed(context, seedPasswordHasher.Hash))

    .UseAsyncSeeding(async (context, _, ct) => await FleetSeeder.SeedAsync(context, seedPasswordHasher.Hash, ct)));

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

// Seguridad: la configuración y el generador de tokens viven durante toda la aplicación.
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Las imágenes se guardan en disco y su referencia queda en la base de datos.
var imagesRelativePath = builder.Configuration["Storage:VehicleImagesPath"] ?? "wwwroot/uploads/vehicles";
var publicPrefix = imagesRelativePath
    .Replace('\\', '/')
    .Trim('/')
    .Replace("wwwroot/", string.Empty);

builder.Services.AddSingleton<IImageStorage>(_ =>
    new LocalImageStorage(builder.Environment.ContentRootPath, imagesRelativePath, publicPrefix));

// Bitácora de peticiones en la carpeta configurada, relativa a la WebAPI.
var requestLogDirectory = builder.Configuration["RequestLog:Directory"] ?? "Logs";
var requestLogPath = Path.IsPathRooted(requestLogDirectory)
    ? requestLogDirectory
    : Path.Combine(builder.Environment.ContentRootPath, requestLogDirectory);
builder.Services.AddSingleton<IFileLogger>(new FileRequestLogger(requestLogPath));

// Se construye la aplicación y se registra el manejo de errores HTTP.
var app = builder.Build();

// Los archivos estáticos se sirven antes de la bitácora y por eso no generan registros.
app.UseStaticFiles();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors("Frontend");
app.UseExceptionHandler();
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    var statusCode = httpContext.Response.StatusCode;
    string title;
    string detail;

    switch (statusCode)
    {
        case StatusCodes.Status401Unauthorized:
            title = "Unauthorized";
            detail = "Sesión no válida o expirada. Inicie sesión nuevamente.";
            break;
        case StatusCodes.Status403Forbidden:
            title = "Forbidden";
            detail = "No tiene permisos para realizar esta acción.";
            break;
        case StatusCodes.Status404NotFound:
            title = "Not Found";
            detail = "Recurso no encontrado.";
            break;
        case StatusCodes.Status405MethodNotAllowed:
            title = "Method Not Allowed";
            detail = "Método no permitido.";
            break;
        default:
            title = "Error";
            detail = "Solicitud no válida.";
            break;
    }

    var result = Result<object>.Failure(statusCode, title, detail);
    httpContext.Items[RequestLoggingMiddleware.ErrorItemKey] = result;
    httpContext.Response.ContentType = "application/json; charset=utf-8";
    await httpContext.Response.WriteAsJsonAsync(result);
});

// Scalar queda disponible al ejecutar en desarrollo.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Las migraciones se aplican por separado, no al arrancar la API.
app.Run();
