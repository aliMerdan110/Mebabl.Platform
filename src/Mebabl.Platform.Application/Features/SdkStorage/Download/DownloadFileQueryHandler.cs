using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Download;

public sealed class DownloadFileQueryHandler
    : IRequestHandler<
        DownloadFileQuery,
        DownloadFileResult>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly IStorageProvider _storage;

    public DownloadFileQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        IStorageProvider storage)
    {
        _db = db;
        _currentApplication = currentApplication;
        _storage = storage;
    }

    public async Task<DownloadFileResult> Handle(
        DownloadFileQuery request,
        CancellationToken cancellationToken)
    {
        var applicationId =
            _currentApplication.ApplicationId;

        var file = await _db.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.FileId &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (file is null)
            throw new KeyNotFoundException(
                "File was not found.");

        var stream = await _storage.OpenReadAsync(
            file.StorageKey,
            cancellationToken);

        return new DownloadFileResult(
            stream,
            file.ContentType,
            file.Name);
    }
}