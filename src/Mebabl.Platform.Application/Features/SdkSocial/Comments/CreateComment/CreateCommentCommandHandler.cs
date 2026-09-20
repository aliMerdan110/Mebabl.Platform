using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.CreateComment;

public sealed class CreateCommentCommandHandler
    : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public CreateCommentCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<CommentDto> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Comment text is required.");

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var postExists = await _db.Posts
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.PostId &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (!postExists)
            throw new KeyNotFoundException("Post not found.");

        var comment = new SocialComment
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            PostId = request.PostId,
            OwnerId = userId,
            Text = request.Text.Trim()
        };

        _db.SocialComments.Add(comment);

        await _db.SaveChangesAsync(cancellationToken);

        return new CommentDto(
            comment.Id,
            comment.PostId,
            comment.OwnerId,
            comment.ParentCommentId,
            comment.Text,
            comment.CreatedAt,
            comment.UpdatedAt);
    }
}