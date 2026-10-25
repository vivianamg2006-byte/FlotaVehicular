using System.Diagnostics;
using System.Text.Json;
using FlotaVehicular.Domain.DTO.PatternResult;
using FlotaVehicular.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FlotaVehicular.WebAPI.Handlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        int status;
        string title;
        string detail;
        object? data = null;

        if (exception is AppException appException)
        {
            status = appException.Status;
            title = appException.Title;
            detail = appException.Message;
        }
        else if (exception is BadHttpRequestException badRequest)
        {
            status = badRequest.StatusCode;
            title = "Bad Request";
            detail = "La solicitud no es válida.";
        }
        else if (exception is JsonException)
        {
            status = StatusCodes.Status400BadRequest;
            title = "Bad Request";
            detail = "El cuerpo JSON no es válido.";
        }
        else if (exception is DbUpdateConcurrencyException)
        {
            status = StatusCodes.Status409Conflict;
            title = "Conflict";
            detail = "El registro fue modificado por otro usuario.";
        }
        else if (exception is DbUpdateException dbUpdateException)
        {
            if (dbUpdateException.InnerException is SqlException sqlException)
            {
                if (sqlException.Number == 2601 || sqlException.Number == 2627)
                {
                    status = StatusCodes.Status409Conflict;
                    title = "Conflict";
                    detail = "No se pudo guardar porque existe un registro duplicado.";
                }
                else if (sqlException.Number == 547)
                {
                    status = StatusCodes.Status409Conflict;
                    title = "Conflict";
                    detail = "No se pudo guardar por restricción de integridad referencial.";
                }
                else
                {
                    status = StatusCodes.Status500InternalServerError;
                    title = "Internal Server Error";
                    detail = "Ocurrió un error inesperado.";
                    data = new { traceId };
                }
            }
            else
            {
                status = StatusCodes.Status500InternalServerError;
                title = "Internal Server Error";
                detail = "Ocurrió un error inesperado.";
                data = new { traceId };
            }
        }
        else
        {
            status = StatusCodes.Status500InternalServerError;
            title = "Internal Server Error";
            detail = "Ocurrió un error inesperado.";
            data = new { traceId };
        }

        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado {TraceId} en {Method} {Path}", traceId, httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Excepción convertida en {StatusCode} {TraceId} en {Method} {Path}", status, traceId, httpContext.Request.Method, httpContext.Request.Path);
        }

        var result = Result<object>.Failure(status, title, detail);
        if (data is not null)
        {
            result.Data = data;
        }

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken: cancellationToken);

        return true;
    }
}
