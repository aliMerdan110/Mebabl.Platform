using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkPosts.DTOs;

namespace Mebabl.Platform.Application.Features.SdkPosts.List;

public sealed class ListPostsQueryHandler
    : IRequestHandler<
        ListPostsQuery,
        IReadOnlyList<PostDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public ListPostsQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<PostDto>> Handle(
        ListPostsQuery request,
        CancellationToken cancellationToken)
    {
        var skip = Math.Max(request.Skip, 0);
        var take = Math.Clamp(request.Take, 1, 100);

        var applicationId = _currentApplication.ApplicationId;

        return await _db.Posts
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => new PostDto(
                x.Id,
                x.OwnerId,
                x.Text,
                x.CreatedAt,
                x.Attachments
                    .OrderBy(a => a.Order)
                    .Select(a => new PostAttachmentDto(
                        a.Id,
                        a.StorageFileId,
                        a.Type,
                        a.Order,
                        a.StorageFile.Name,
                        a.StorageFile.Path,
                        a.StorageFile.ContentType,
                        a.StorageFile.Size,
                        a.StorageFile.IsPublic,
                        a.StorageFile.CreatedAt,
                        $"/storage/files/{a.StorageFile.Id}"))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}