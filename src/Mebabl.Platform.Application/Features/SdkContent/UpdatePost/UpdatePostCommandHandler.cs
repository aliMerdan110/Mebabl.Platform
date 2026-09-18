using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkContent.DTOs;

namespace Mebabl.Platform.Application.Features.SdkContent.UpdatePost;

public sealed class UpdatePostCommandHandler
    : IRequestHandler<UpdatePostCommand, PostDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public UpdatePostCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<PostDto> Handle(
        UpdatePostCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "Authenticated user is required.");

        var post = await _db.Posts
            .FirstOrDefaultAsync(x =>
                x.Id == request.PostId &&
                x.ApplicationId == applicationId &&
                x.OwnerId == userId &&
                !x.IsDeleted,
                cancellationToken);

        if (post is null)
            throw new KeyNotFoundException("Post not found.");

        var text = string.IsNullOrWhiteSpace(request.Text)
            ? null
            : request.Text.Trim();

        if (text is null && !post.Attachments.Any())
            throw new ArgumentException(
                "Post must contain text or at least one attachment.");

        post.Text = text;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return await _db.Posts
            .AsNoTracking()
            .Where(x =>
                x.Id == post.Id &&
                x.ApplicationId == applicationId)
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
            .FirstAsync(cancellationToken);
    }
}