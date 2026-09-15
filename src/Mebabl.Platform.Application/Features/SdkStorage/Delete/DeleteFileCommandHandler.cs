using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Delete;

public sealed class DeleteFileCommandHandler
    : IRequestHandler<DeleteFileCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IStorageProvider _storage;

    public DeleteFileCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IStorageProvider storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task Handle(
        DeleteFileCommand request,
        CancellationToken cancellationToken)
    {
        var file = await _db.StoredFiles
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.FileId &&
                    x.ApplicationId == _currentUser.ApplicationId &&
                    x.UserId == _currentUser.UserId &&
                    !x.IsDeleted,
                cancellationToken);

        if (file is null)
            throw new KeyNotFoundException(
                "Storage file not found.");

        await _storage.DeleteAsync(
            file.StorageKey,
            cancellationToken);

        file.IsDeleted = true;
        file.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(
            cancellationToken);
    }
}