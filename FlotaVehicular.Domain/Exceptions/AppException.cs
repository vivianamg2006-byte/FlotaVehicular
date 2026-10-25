namespace FlotaVehicular.Domain.Exceptions;

public class AppException : Exception
{
    public int Status { get; }
    public string Title { get; }

    public AppException(int status, string title, string detail) : base(detail)
    {
        Status = status;
        Title = title;
    }
}
