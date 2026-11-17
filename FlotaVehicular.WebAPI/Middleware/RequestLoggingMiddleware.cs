using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FlotaVehicular.Infrastructure.Logger;

namespace FlotaVehicular.WebAPI.Middleware;

// Registra cada llamada a /api sin guardar cuerpos ni el encabezado Authorization.
public sealed class RequestLoggingMiddleware
{
    public const string ErrorItemKey = "RequestLog.Error";
    public const string ExceptionItemKey = "RequestLog.Exception";
    public const string TraceIdItemKey = "RequestLog.TraceId";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly IFileLogger _fileLogger;

    public RequestLoggingMiddleware(RequestDelegate next, IFileLogger fileLogger)
    {
        _next = next;
        _fileLogger = fileLogger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldLog(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            await WriteLogAsync(context, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    // Solo entran las peticiones de la API; Scalar, OpenAPI, estáticos y preflight quedan fuera.
    private static bool ShouldLog(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            return false;

        var isPreflight = HttpMethods.IsOptions(context.Request.Method)
            && context.Request.Headers.ContainsKey("Access-Control-Request-Method");

        return !isPreflight;
    }

    private async Task WriteLogAsync(HttpContext context, double elapsedMilliseconds)
    {
        try
        {
            var traceId = context.Items[TraceIdItemKey] as string
                ?? Activity.Current?.Id
                ?? context.TraceIdentifier;

            var statusCode = context.Response.StatusCode;
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;
            var user = context.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated == true;

            var content = new StringBuilder();
            content.AppendLine($"Fecha y hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            content.AppendLine($"TraceId: {traceId}");
            content.AppendLine($"Método: {method}");
            content.AppendLine($"Ruta: {path}");
            content.AppendLine($"Parámetros de consulta: {context.Request.QueryString.Value}");
            content.AppendLine($"Código HTTP: {statusCode}");
            content.AppendLine($"Resultado: {(statusCode < 400 ? "Exitoso" : "Error")}");
            content.AppendLine($"Duración (ms): {elapsedMilliseconds:0.###}");
            content.AppendLine($"IP del cliente: {context.Connection.RemoteIpAddress?.ToString() ?? "desconocida"}");
            content.AppendLine(isAuthenticated
                ? $"Usuario: {user!.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "desconocido"}"
                : "Usuario: anónimo");
            content.AppendLine(isAuthenticated
                ? $"Rol: {user!.FindFirst(ClaimTypes.Role)?.Value ?? "sin rol"}"
                : "Rol: anónimo");

            if (statusCode >= 400)
                AppendError(content, context);

            var fileName = BuildFileName(DateTime.Now, method, path, statusCode);
            await _fileLogger.WriteAsync(fileName, content.ToString());
        }
        catch
        {
            // La bitácora nunca debe alterar la respuesta que ya recibió el cliente.
        }
    }

    private static void AppendError(StringBuilder content, HttpContext context)
    {
        if (context.Items[ExceptionItemKey] is Exception exception)
            content.AppendLine($"Excepción: {exception}");
        else if (context.Items[ErrorItemKey] is { } error)
            content.AppendLine($"Result de error: {JsonSerializer.Serialize(error, JsonOptions)}");
    }

    private static string BuildFileName(DateTime timestamp, string method, string path, int statusCode)
    {
        var route = path.Trim('/').Replace('/', '-');
        foreach (var invalid in Path.GetInvalidFileNameChars())
            route = route.Replace(invalid, '-');

        if (route.Length == 0)
            route = "api";

        if (route.Length > 80)
            route = route[..80];

        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(3)).ToLowerInvariant();
        return $"{timestamp:yyyyMMdd_HHmmss_fff}_{method}_{route}_{statusCode}_{code}.txt";
    }
}
