using FlotaVehicular.Infrastructure.Interfaces;

namespace FlotaVehicular.Infrastructure.Implementation;

public class LocalImageStorage : IImageStorage
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private readonly string _contentRoot;
    private readonly string _rootPath;
    private readonly string _relativeRoot;
    private readonly string _publicPrefix;

    public LocalImageStorage(string contentRootPath, string relativeRoot, string publicPrefix)
    {
        _contentRoot = Path.GetFullPath(contentRootPath);
        _relativeRoot = relativeRoot.Replace('\\', '/').Trim('/');
        _rootPath = Path.GetFullPath(Path.Combine(_contentRoot, _relativeRoot));
        _publicPrefix = "/" + publicPrefix.Trim('/');

        Directory.CreateDirectory(_rootPath);
    }

    public Task<string> SaveAsync(string originalFileName, byte[] content, CancellationToken ct = default)
    {
        // El nombre generado evita sobrescribir otro archivo.
        var fileName = $"{Guid.NewGuid():N}{SafeExtension(originalFileName)}";

        File.WriteAllBytes(Path.Combine(_rootPath, fileName), content);

        return Task.FromResult($"{_relativeRoot}/{fileName}");
    }

    public Task DeleteAsync(string relativePath)
    {
        var fullPath = ResolvePath(relativePath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<byte[]> ReadAllBytesAsync(string relativePath)
        => File.ReadAllBytesAsync(ResolvePath(relativePath));

    public string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("La ruta del archivo está vacía.", nameof(relativePath));

        var fullPath = Path.GetFullPath(Path.Combine(_contentRoot, relativePath));

        // Solo se permite acceder a archivos dentro de la carpeta de imágenes.
        if (!fullPath.StartsWith(_rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(fullPath, _rootPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La ruta del archivo está fuera del directorio permitido.");

        return fullPath;
    }

    public string ToPublicUrl(string relativePath)
        => $"{_publicPrefix}/{Path.GetFileName(relativePath)}";

    private static string SafeExtension(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName ?? string.Empty).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"Extensión no permitida: '{extension}'.");

        return extension;
    }
}
