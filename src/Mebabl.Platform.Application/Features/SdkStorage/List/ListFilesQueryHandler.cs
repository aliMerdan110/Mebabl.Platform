using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.List;

public sealed class ListFilesQueryHandler
    : IRequestHandler<ListFilesQuery, IReadOnlyList<StorageFileDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ListFilesQueryHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<StorageFileDto>> Handle(
        ListFilesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.StoredFiles
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == _currentUser.ApplicationId &&
                x.UserId == _currentUser.UserId &&
                !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Path))
        {
            var path = request.Path.Trim('/');

            query = query.Where(x =>
                x.Path == path);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(file => new StorageFileDto(
                file.Id,
                file.Name,
                file.Path,
                file.ContentType,
                file.Size,
                file.IsPublic,
                file.CreatedAt,
                $"/storage/files/{file.Id}",
                file.UserId
            ))
            .ToListAsync(cancellationToken);
    }
}