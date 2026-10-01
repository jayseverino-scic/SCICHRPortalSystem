namespace SCICHRPortal.Service.Interfaces;

// Keys are opaque to callers. A future blob provider implements this same contract.
public interface ITimeLogAttachmentStorage
{
    string Provider { get; }
    Task<string> SaveAsync(Stream source, CancellationToken cancellationToken = default);
    Task<Stream> OpenAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
