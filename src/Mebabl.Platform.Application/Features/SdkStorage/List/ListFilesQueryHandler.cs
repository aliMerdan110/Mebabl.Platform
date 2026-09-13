using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.List;

public sealed class ListFilesQueryHandler
    : IRequestHandler<
        ListFilesQuery,
        IReadOnlyList<StorageFileDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public ListFilesQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<StorageFileDto>> Handle(
        ListFilesQuery request,
        CancellationToken cancellationToken)
    {
        var applicationId =
            _currentApplication.ApplicationId;

        var query = _db.StoredFiles
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId);

        if (!string.IsNullOrWhiteSpace(request.Path))
        {
            var path = request.Path.Trim('/');

            query = query.Where(x =>
                x.Path == path ||
                x.Path.StartsWith(path + "/"));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new StorageFileDto(
                x.Id,
                x.Path,
                x.Name,
                x.ContentType,
                x.Size,
                x.IsPublic,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}