using System.Text.Json.Serialization;

namespace FlotaVehicular.Domain.DTO.PatternResult;

// Respuesta estándar de la API: contenido de la operación y, cuando aplica,
// datos de paginación.
public class Result<T> : BaseResult
{
    // Contenido de la respuesta; en errores puede ir el detalle por campo.
    [JsonPropertyOrder(1)]
    public T? Data { get; set; }

    // Número de página solicitado; solo se envía en respuestas paginadas.
    [JsonPropertyOrder(2), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageNumber { get; set; }

    // Tamaño de página solicitado; solo se envía en respuestas paginadas.
    [JsonPropertyOrder(3), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageSize { get; set; }

    // Total de registros que coincide con el filtro aplicado.
    [JsonPropertyOrder(4), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalRecords { get; set; }

    // Páginas totales calculadas a partir del total de registros.
    [JsonPropertyOrder(5), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalPages { get; set; }

    // Crea una respuesta exitosa con el contenido de la operación.
    public static Result<T> Success(T data, int status = 200, string title = "OK", string detail = "") =>
        new() { IsSuccess = true, Status = status, Title = title, Detail = detail, Data = data };

    // Crea una respuesta fallida con el estado, el título y el detalle del error.
    public static Result<T> Failure(int status, string title, string detail) =>
        new() { IsSuccess = false, Status = status, Title = title, Detail = detail };

    // Crea una respuesta exitosa con los datos de paginación.
    public static Result<T> Paged(T data, int pageNumber, int pageSize, int totalRecords) =>
        new()
        {
            IsSuccess = true,
            Status = 200,
            Title = "OK",
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        };
}
