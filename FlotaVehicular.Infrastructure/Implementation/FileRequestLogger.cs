using FlotaVehicular.Infrastructure.Logger;

namespace FlotaVehicular.Infrastructure.Implementation;

// Guarda cada bitácora como un archivo de texto independiente.
public sealed class FileRequestLogger : IFileLogger
{
    private readonly string _directory;

    public FileRequestLogger(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(_directory);
    }

    // La carpeta se crea de nuevo por si se eliminó mientras la API estaba activa.
    public async Task WriteAsync(
        string fileName, string content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var fullPath = Path.Combine(_directory, fileName);
        await File.AppendAllTextAsync(fullPath, content, cancellationToken);
    }
}
