using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkContent.DTOs;

namespace Mebabl.Platform.Application.Features.SdkContent.GetPosts;

public sealed class GetPostsQueryHandler
    : IRequestHandler<GetPostsQuery, IReadOnlyList<PostDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public GetPostsQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<PostDto>> Handle(
        GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return await _db.Posts
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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