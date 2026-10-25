namespace FlotaVehicular.Domain.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string detail) : base(409, "Conflict", detail)
    {
    }
}
