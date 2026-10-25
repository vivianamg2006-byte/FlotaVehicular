namespace FlotaVehicular.Domain.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string detail) : base(404, "Not Found", detail)
    {
    }
}
