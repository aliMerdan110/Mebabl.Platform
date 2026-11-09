using Microsoft.Extensions.Logging;

using MediatR;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Upload;

public sealed class UploadFileCommandHandler
    : IRequestHandler<UploadFileCommand, StoredFile>
{
    private const long MaximumFileSize = 50L * 1024 * 1024;
    private const int MaximumPathLength = 1024;
    private const int MaximumFileNameLength = 255;

    private readonly IApplicationDbContext _db;
    private readonly IStorageProvider _storage;
    private readonly ILogger<UploadFileCommandHandler> _logger;

    public UploadFileCommandHandler(
        IApplicationDbContext db,
        IStorageProvider storage,
        ILogger<UploadFileCommandHandler> logger)
    {
        _db = db;
        _storage = storage;
        _logger = logger;
    }

    public async Task<StoredFile> Handle(
        UploadFileCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ApplicationId == Guid.Empty ||
            request.UserId is null ||
            request.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "A valid application and user are required.");
        }

        ArgumentNullException.ThrowIfNull(request.Content);

        if (!request.Content.CanSeek)
        {
            throw new ArgumentException(
                "The upload stream must support seeking.");
        }

        var suppliedFileName = request.FileName?.Trim();

        if (string.IsNullOrWhiteSpace(suppliedFileName))
        {
            throw new ArgumentException("A file name is required.");
        }

        var fileName = Path.GetFileName(
            suppliedFileName.Replace('\\', '/'));

        if (fileName != suppliedFileName ||
            fileName is "." or ".." ||
            fileName.Length > MaximumFileNameLength ||
            fileName.Any(char.IsControl))
        {
            throw new ArgumentException("The file name is invalid.");
        }

        if (!TryNormalizePath(request.Path, out var normalizedPath))
        {
            throw new ArgumentException("The storage path is invalid.");
        }

        var contentType = string.IsNullOrWhiteSpace(request.ContentType)
            ? "application/octet-stream"
            : request.ContentType.Trim();

        if (contentType.Length > 255 ||
            contentType.Any(char.IsControl))
        {
            throw new ArgumentException("The content type is invalid.");
        }

        var extension = Path.GetExtension(fileName);

        if (extension.Length > 17 ||
            extension.Skip(1).Any(c => !char.IsAsciiLetterOrDigit(c)))
        {
            throw new ArgumentException("The file extension is invalid.");
        }

        extension = extension.ToLowerInvariant();

        var size = request.Content.Length;

        if (size <= 0 || size > MaximumFileSize)
        {
            throw new ArgumentException(
                "The file size must be between 1 byte and 50 MB.");
        }

        request.Content.Position = 0;

        var id = Guid.NewGuid();

        var storageKey =
            $"{request.ApplicationId:N}/{id:N}{extension}";

        var file = new StoredFile
        {
            Id = id,
            ApplicationId = request.ApplicationId,
            UserId = request.UserId,
            Path = normalizedPath,
            FileName = fileName,
            Name = Path.GetFileNameWithoutExtension(fileName),
            ContentType = contentType,
            Extension = extension,
            Size = size,
            StorageKey = storageKey,
            IsPublic = request.IsPublic,
            Version = 1
        };

        try
        {
            await _storage.WriteAsync(
                storageKey,
                request.Content,
                cancellationToken);

            _db.StoredFiles.Add(file);

            await _db.SaveChangesAsync(cancellationToken);

            return file;
        }
        catch
        {
            // محاولة تنظيف الملف إذا فشل التخزين أو حفظ بياناته.
            try
            {
                await _storage.DeleteAsync(
                    storageKey,
                    CancellationToken.None);
            }
            catch (Exception cleanupException)
            {
                _logger.LogError(
                    cleanupException,
                    "Failed to clean up storage object {StorageKey}.",
                    storageKey);
            }

            throw;
        }
    }

    private static bool TryNormalizePath(
        string? path,
        out string normalizedPath)
    {
        normalizedPath = string.Empty;

        if (string.IsNullOrWhiteSpace(path) ||
            path.Length > MaximumPathLength ||
            path.Contains('\0'))
        {
            return false;
        }

        var candidate = path.Trim().Replace('\\', '/');

        if (candidate.StartsWith('/') ||
            (candidate.Length >= 2 &&
             char.IsLetter(candidate[0]) &&
             candidate[1] == ':'))
        {
            return false;
        }

        var segments = candidate.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        if (segments.Length == 0 ||
            segments.Any(segment =>
                segment is "." or ".." ||
                segment.Any(char.IsControl)))
        {
            return false;
        }

        normalizedPath = string.Join('/', segments);

        return normalizedPath.Length <= MaximumPathLength;
    }
}