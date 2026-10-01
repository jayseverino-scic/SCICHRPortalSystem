using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Storage;

public sealed class LocalTimeLogAttachmentStorage(string root) : ITimeLogAttachmentStorage
{
    private readonly string directory = Path.GetFullPath(root);
    public string Provider => "Local";

    public async Task<string> SaveAsync(Stream source, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(directory);
        var key = Guid.NewGuid().ToString("N");
        var path = Resolve(key);
        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            await source.CopyToAsync(output, cancellationToken);
            return key;
        }
        catch
        {
            File.Delete(path);
            throw;
        }
    }

    public Task<Stream> OpenAsync(string key, CancellationToken cancellationToken = default) =>
        Task.FromResult<Stream>(new FileStream(Resolve(key), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true));

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        File.Delete(Resolve(key));
        return Task.CompletedTask;
    }

    private string Resolve(string key)
    {
        if (!Guid.TryParseExact(key, "N", out _)) throw new ArgumentException("Invalid attachment storage key.", nameof(key));
        return Path.Combine(directory, key);
    }
}
