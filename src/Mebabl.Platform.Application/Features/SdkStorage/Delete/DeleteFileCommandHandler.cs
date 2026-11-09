using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Delete;

public sealed class DeleteFileCommandHandler
    : IRequestHandler<DeleteFileCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageProvider _storage;

    public DeleteFileCommandHandler(
        IApplicationDbContext db,
        IStorageProvider storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task Handle(
        DeleteFileCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ApplicationId == Guid.Empty ||
            request.UserId == Guid.Empty ||
            request.FileId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Valid application, user and file identifiers are required.");
        }

        var file = await _db.StoredFiles
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.FileId &&
                    x.ApplicationId == request.ApplicationId &&
                    x.UserId == request.UserId,
                cancellationToken);

        if (file is null)
        {
            throw new KeyNotFoundException("File not found.");
        }

        // لا نحذف بيانات قاعدة البيانات إذا فشل حذف الملف الفعلي.
        await _storage.DeleteAsync(
            file.StorageKey,
            cancellationToken);

        _db.StoredFiles.Remove(file);

        await _db.SaveChangesAsync(cancellationToken);
    }
}