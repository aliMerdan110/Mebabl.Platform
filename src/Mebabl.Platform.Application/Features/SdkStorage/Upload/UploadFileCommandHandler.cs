using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Upload;

public sealed class UploadFileCommandHandler
    : IRequestHandler<UploadFileCommand, StoredFile>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageProvider _storage;

    public UploadFileCommandHandler(
        IApplicationDbContext db,
        IStorageProvider storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<StoredFile> Handle(
        UploadFileCommand request,
        CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        var extension = Path.GetExtension(request.FileName);

        var storageKey =
            $"{request.ApplicationId:N}/{id:N}{extension}";

        await _storage.WriteAsync(
            storageKey,
            request.Content,
            cancellationToken);

        var file = new StoredFile
        {
            Id = id,
            ApplicationId = request.ApplicationId,
            UserId = request.UserId,
            Path = request.Path,
            FileName = request.FileName,
            Name = Path.GetFileNameWithoutExtension(request.FileName),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType)
                ? "application/octet-stream"
                : request.ContentType,
            Extension = extension,
            Size = request.Content.CanSeek
                ? request.Content.Length
                : 0,
            StorageKey = storageKey,
            IsPublic = request.IsPublic,
            Version = 1
        };

        _db.StoredFiles.Add(file);
        await _db.SaveChangesAsync(cancellationToken);

        return file;
    }
}
