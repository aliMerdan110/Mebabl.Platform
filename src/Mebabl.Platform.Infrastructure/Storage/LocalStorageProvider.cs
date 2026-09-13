using Microsoft.Extensions.Hosting;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Infrastructure.Storage;

public sealed class LocalStorageProvider : IStorageProvider
{
    private readonly string _rootPath;

    public LocalStorageProvider(IHostEnvironment environment)
    {
        _rootPath = Path.Combine(environment.ContentRootPath, "storage");
        Directory.CreateDirectory(_rootPath);
    }

    public Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var path = GetSafePath(storageKey);

        Stream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            useAsync: true);

        return Task.FromResult(stream);
    }

    public async Task WriteAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var path = GetSafePath(storageKey);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            64 * 1024,
            useAsync: true);

        await content.CopyToAsync(file, cancellationToken);
    }

    public Task DeleteAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        var path = GetSafePath(storageKey);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            File.Exists(GetSafePath(storageKey)));
    }

    private string GetSafePath(string storageKey)
    {
        var normalized = storageKey
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(_rootPath, normalized));

        var root = Path.GetFullPath(_rootPath)
            .TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
                root,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid storage path.");
        }

        return fullPath;
    }
}