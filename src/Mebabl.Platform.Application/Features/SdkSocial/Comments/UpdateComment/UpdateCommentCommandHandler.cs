using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.UpdateComment;

public sealed class UpdateCommentCommandHandler
    : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private const int MaximumCommentLength = 5000;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public UpdateCommentCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CommentDto> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        if (applicationId == Guid.Empty ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        if (request.CommentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Comment ID is required.",
                nameof(request.CommentId));
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ArgumentException(
                "Comment text is required.");
        }

        var text = request.Text.Trim();

        if (text.Length > MaximumCommentLength ||
            text.Contains('\0'))
        {
            throw new ArgumentException(
                $"Comment text cannot exceed {MaximumCommentLength} characters.");
        }

        var comment = await _db.SocialComments
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.CommentId &&
                    x.ApplicationId == applicationId &&
                    x.OwnerId == userId,
                cancellationToken);

        if (comment is null)
        {
            throw new KeyNotFoundException(
                "Comment not found.");
        }

        comment.Text = text;
        comment.UpdatedAt = DateTime.UtcNow;

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