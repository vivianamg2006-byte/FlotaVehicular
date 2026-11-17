namespace FlotaVehicular.Infrastructure.Logger;

// Escribe las bitácoras de peticiones en el sistema de archivos.
public interface IFileLogger
{
    Task WriteAsync(string fileName, string content, CancellationToken cancellationToken = default);
}
