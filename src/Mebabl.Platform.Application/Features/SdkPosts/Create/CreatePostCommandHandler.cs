using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkPosts.DTOs;
using Mebabl.Platform.Domain.Entities.Content;

namespace Mebabl.Platform.Application.Features.SdkPosts.Create;

public sealed class CreatePostCommandHandler
    : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public CreatePostCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<PostDto> Handle(
        CreatePostCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;
        var ownerId = _currentUser.UserId;

        if (ownerId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "Authenticated user is required.");

        var text = string.IsNullOrWhiteSpace(request.Text)
            ? null
            : request.Text.Trim();

        var fileIds = request.StorageFileIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList()
            ?? new List<Guid>();

        if (text is null && fileIds.Count == 0)
            throw new ArgumentException(
                "Post must contain text or at least one attachment.");

        var files = fileIds.Count == 0
            ? new List<Domain.Entities.Storage.StoredFile>()
            : await _db.StoredFiles
                .Where(x =>
                    fileIds.Contains(x.Id) &&
                    x.ApplicationId == applicationId &&
                    x.UserId == ownerId &&
                    !x.IsDeleted)
                .ToListAsync(cancellationToken);

        if (files.Count != fileIds.Count)
            throw new InvalidOperationException(
                "One or more attachments do not belong to the current user.");

        var post = new Post
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            OwnerId = ownerId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            Attachments = new List<PostAttachment>()
        };

        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];

            post.Attachments.Add(new PostAttachment
            {
                Id = Guid.NewGuid(),
                PostId = post.Id,
                StorageFileId = file.Id,
                Type = ResolveType(file.ContentType),
                Order = index,
                CreatedAt = DateTime.UtcNow
            });
        }

        _db.Posts.Add(post);

        await _db.SaveChangesAsync(cancellationToken);

        var result = await _db.Posts
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

        return result;
    }

    private static string ResolveType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return "File";

        if (contentType.StartsWith(
                "image/",
                StringComparison.OrdinalIgnoreCase))
            return "Image";

        if (contentType.StartsWith(
                "video/",
                StringComparison.OrdinalIgnoreCase))
            return "Video";

        if (contentType.StartsWith(
                "audio/",
                StringComparison.OrdinalIgnoreCase))
            return "Audio";

        return "File";
    }
}