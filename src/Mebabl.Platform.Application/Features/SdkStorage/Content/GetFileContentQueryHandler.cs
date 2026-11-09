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
        if (request.ApplicationId == Guid.Empty ||
            request.UserId == Guid.Empty ||
            request.FileId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Valid application, user and file identifiers are required.");
        }

        var file = await _db.StoredFiles
            .AsNoTracking()
            .Where(x =>
                x.Id == request.FileId &&
                x.ApplicationId == request.ApplicationId &&
                (x.IsPublic || x.UserId == request.UserId))
            .Select(x => new
            {
                x.StorageKey,
                x.ContentType,
                x.FileName,
                x.Size
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (file is null)
        {
            throw new KeyNotFoundException("File not found.");
        }

        var stream = await _storage.OpenReadAsync(
            file.StorageKey,
            cancellationToken);

        return new GetFileContentResult(
            stream,
            string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType,
            file.FileName,
            file.Size);
    }
}