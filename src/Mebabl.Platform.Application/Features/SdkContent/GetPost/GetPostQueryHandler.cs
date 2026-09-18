using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkContent.DTOs;

namespace Mebabl.Platform.Application.Features.SdkContent.GetPost;

public sealed class GetPostQueryHandler
    : IRequestHandler<GetPostQuery, PostDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public GetPostQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<PostDto> Handle(
        GetPostQuery request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        var post = await _db.Posts
            .AsNoTracking()
            .Where(x =>
                x.Id == request.PostId &&
                x.ApplicationId == applicationId &&
                !x.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken);

        return post
            ?? throw new KeyNotFoundException("Post not found.");
    }
}