using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Upload;

public sealed class UploadFileCommandHandler
    : IRequestHandler<UploadFileCommand, StorageFileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IStorageProvider _storage;

    public UploadFileCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IStorageProvider storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<StorageFileDto> Handle(
        UploadFileCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId =
            _currentUser.ApplicationId;

        var ownerId =
            _currentUser.UserId;

        var normalizedPath =
            request.Path.Trim('/');

        var extension =
            System.IO.Path.GetExtension(
                request.FileName);

        var storageKey =
            $"{applicationId}/{normalizedPath}/{Guid.NewGuid():N}{extension}";

        await _storage.WriteAsync(
            storageKey,
            request.Content,
            cancellationToken);

        var file = new StoredFile
        {
            ApplicationId = applicationId,
            UserId = ownerId,
            Name = request.FileName,
            Path = request.Path,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Extension = extension,
            Size = request.Size,
            StorageKey = storageKey,
            IsPublic = request.IsPublic,
            IsDeleted = false,
            Version = 1
        };

        _db.StoredFiles.Add(file);

        await _db.SaveChangesAsync(
            cancellationToken);

        return new StorageFileDto(
            file.Id,
            file.Name,
            file.Path,
            file.ContentType,
            file.Size,
            file.IsPublic,
            file.CreatedAt,
            $"/storage/files/{file.Id}",
            file.UserId
        );
    }
}