using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Application.Common.Storage;

public interface IStorageProvider
{
    Task<StorageResult> SaveAsync(
        Bucket bucket,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        StoredFile file,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        StoredFile file,
        CancellationToken cancellationToken);
}