using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Download;

public sealed class DownloadFileQueryHandler
    : IRequestHandler<
        DownloadFileQuery,
        DownloadFileResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IStorageProvider _storage;

    public DownloadFileQueryHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IStorageProvider storage)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
    }

    public async Task<DownloadFileResult> Handle(
        DownloadFileQuery request,
        CancellationToken cancellationToken)
    {
        var file = await _db.StoredFiles
            .AsNoTracking()
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

        if (!await _storage.ExistsAsync(
                file.StorageKey,
                cancellationToken))
        {
            throw new FileNotFoundException(
                "Storage object not found.");
        }

        var stream = await _storage.OpenReadAsync(
            file.StorageKey,
            cancellationToken);

        return new DownloadFileResult(
            stream,
            file.ContentType,
            file.FileName);
    }
}