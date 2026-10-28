namespace FlotaVehicular.Infrastructure.Interfaces;

// Contrato para guardar, leer y eliminar los archivos de imagen en disco.
public interface IImageStorage
{
    Task<string> SaveAsync(string originalFileName, byte[] content, CancellationToken ct = default);

    Task DeleteAsync(string relativePath);

    Task<byte[]> ReadAllBytesAsync(string relativePath);

    string ResolvePath(string relativePath);

    string ToPublicUrl(string relativePath);
}
