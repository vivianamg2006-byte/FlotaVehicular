namespace FlotaVehicular.Domain.Exceptions;

// Error que se devuelve cuando la operación entra en conflicto con el estado actual
// del registro, por ejemplo por un dato duplicado.
public class ConflictException : AppException
{
    public ConflictException(string detail) : base(409, "Conflict", detail)
    {
    }
}
