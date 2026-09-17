using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Application.Features.SdkStorage.Content;

public sealed class GetFileContentQueryHandler
    : IRequestHandler<GetFileContentQuery, GetFileContentResult>
{
    private readonly IApplicationDbContext _db;
    private readonly IStorageProvider _storage;

    public GetFileContentQueryHandler(
        IApplicationDbContext db,
        IStorageProvider storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<GetFileContentResult> Handle(
        GetFileContentQuery request,
        CancellationToken cancellationToken)
    {
        var file = await _db.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.FileId &&
                    x.ApplicationId == request.ApplicationId,
                cancellationToken);

        if (file is null)
            throw new KeyNotFoundException("File not found.");

        var stream = await _storage.OpenReadAsync(
            file.StorageKey,
            cancellationToken);

        return new GetFileContentResult(
            stream,
            file.ContentType,
            file.FileName,
            file.Size);
    }
}