namespace FlotaVehicular.Domain.Exceptions;

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string detail) : base(400, "Bad Request", detail)
    {
    }
}
