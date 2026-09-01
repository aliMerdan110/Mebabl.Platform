using System.Security.Cryptography;
using Mebabl.Platform.Application.Common.Storage;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Infrastructure.Storage;

public sealed class LocalStorageProvider : IStorageProvider
{
    private readonly string _root =
        Path.Combine(AppContext.BaseDirectory, "storage");

    public async Task<StorageResult> SaveAsync(
    Bucket bucket,
    string fileName,
    string contentType,
    Stream content,
    CancellationToken cancellationToken)
{
    Directory.CreateDirectory(_root);

    var bucketFolder = Path.Combine(_root, bucket.Code);

    Directory.CreateDirectory(bucketFolder);

    var key = Guid.NewGuid().ToString("N");

    var extension = Path.GetExtension(fileName);

    var storedFileName = key + extension;

    var fullPath = Path.Combine(bucketFolder, storedFileName);

    await using (var stream = File.Create(fullPath))
    {
        await content.CopyToAsync(stream, cancellationToken);
    }

    string hash;

    await using (var stream = File.OpenRead(fullPath))
    {
        hash = Convert.ToHexString(
            await SHA256.HashDataAsync(stream, cancellationToken));
    }

    return new StorageResult(
        key,
        hash,
        fullPath);
}

    public Task<Stream> OpenReadAsync(
        StoredFile file,
        CancellationToken cancellationToken)
    {
        Stream stream = File.OpenRead(file.StoragePath);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        StoredFile file,
        CancellationToken cancellationToken)
    {
        if (File.Exists(file.StoragePath))
            File.Delete(file.StoragePath);

        return Task.CompletedTask;
    }
}