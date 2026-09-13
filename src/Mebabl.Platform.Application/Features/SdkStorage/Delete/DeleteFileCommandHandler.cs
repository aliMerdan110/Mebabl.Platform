using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Delete;

public sealed class DeleteFileCommandHandler
    : IRequestHandler<DeleteFileCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly IStorageProvider _storage;

    public DeleteFileCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        IStorageProvider storage)
    {
        _db = db;
        _currentApplication = currentApplication;
        _storage = storage;
    }

    public async Task Handle(
        DeleteFileCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId =
            _currentApplication.ApplicationId;

        var file = await _db.StoredFiles
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.FileId &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (file is null)
            throw new KeyNotFoundException(
                "File was not found.");

        await _storage.DeleteAsync(
            file.StorageKey,
            cancellationToken);

        _db.StoredFiles.Remove(file);

        await _db.SaveChangesAsync(cancellationToken);
    }
}