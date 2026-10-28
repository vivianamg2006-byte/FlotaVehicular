namespace FlotaVehicular.Domain.DTO.PatternResult;

// Estructura común que acompaña a las respuestas de la API.
public abstract class BaseResult
{
    // Indica si la operación terminó bien o no.
    public bool IsSuccess { get; set; }

    // Código numérico que se envía en la respuesta.
    public int Status { get; set; }

    // Título corto del resultado, por ejemplo "OK" o "Bad Request".
    public string Title { get; set; } = "";

    // Texto explicativo del resultado o del error.
    public string Detail { get; set; } = "";
}
