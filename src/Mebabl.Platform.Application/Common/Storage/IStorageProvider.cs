namespace Mebabl.Platform.Application.Common.Storage;

public interface IStorageProvider
{
    Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task WriteAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}