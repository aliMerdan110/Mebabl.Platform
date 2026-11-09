using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.DeleteComment;

public sealed class DeleteCommentCommandHandler
    : IRequestHandler<DeleteCommentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public DeleteCommentCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(
        DeleteCommentCommand request,
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

        _db.SocialComments.Remove(comment);

        await _db.SaveChangesAsync(cancellationToken);
    }
}