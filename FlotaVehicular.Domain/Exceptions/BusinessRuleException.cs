namespace FlotaVehicular.Domain.Exceptions;

// Error que se devuelve cuando la operación no cumple una regla de negocio.
public class BusinessRuleException : AppException
{
    public BusinessRuleException(string detail) : base(400, "Bad Request", detail)
    {
    }
}
