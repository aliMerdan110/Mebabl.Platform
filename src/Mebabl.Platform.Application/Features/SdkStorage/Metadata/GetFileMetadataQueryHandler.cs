using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.Metadata;

public sealed class GetFileMetadataQueryHandler
    : IRequestHandler<
        GetFileMetadataQuery,
        StorageFileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetFileMetadataQueryHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<StorageFileDto> Handle(
        GetFileMetadataQuery request,
        CancellationToken cancellationToken)
    {
        var file = await _db.StoredFiles
            .AsNoTracking()
            .Where(x =>
                x.Id == request.FileId &&
                x.ApplicationId == _currentUser.ApplicationId &&
                x.UserId == _currentUser.UserId &&
                !x.IsDeleted)
            .Select(x => new StorageFileDto(
                x.Id,
                x.Name,
                x.Path,
                x.ContentType,
                x.Size,
                x.IsPublic,
                x.CreatedAt,
                $"/storage/files/{x.Id}",
                x.UserId
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return file
            ?? throw new KeyNotFoundException(
                "File was not found.");
    }
}