namespace FlotaVehicular.Domain.Exceptions;

// Error que se devuelve cuando el registro que se busca no existe.
public class NotFoundException : AppException
{
    public NotFoundException(string detail) : base(404, "Not Found", detail)
    {
    }
}
