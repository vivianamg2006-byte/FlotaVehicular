namespace FlotaVehicular.Infrastructure.Interfaces;

public interface IImageStorage
{
    Task<string> SaveAsync(string originalFileName, byte[] content, CancellationToken ct = default);

    Task DeleteAsync(string relativePath);

    Task<byte[]> ReadAllBytesAsync(string relativePath);

    string ResolvePath(string relativePath);

    string ToPublicUrl(string relativePath);
}
