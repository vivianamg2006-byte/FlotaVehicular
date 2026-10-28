namespace FlotaVehicular.Domain.Exceptions;

// Excepción base de la aplicación: ya viene con el código HTTP y el título
// que deben devolverse al cliente en la respuesta.
public class AppException : Exception
{
    // Código HTTP que se devuelve al responder el error.
    public int Status { get; }

    // Título corto del error, legible en la respuesta.
    public string Title { get; }

    // Recibe el código de estado, el título y el detalle del problema.
    public AppException(int status, string title, string detail) : base(detail)
    {
        Status = status;
        Title = title;
    }
}
