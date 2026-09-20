using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.DeleteComment;

public sealed class DeleteCommentCommandHandler
    : IRequestHandler<DeleteCommentCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public DeleteCommentCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        DeleteCommentCommand request,
        CancellationToken cancellationToken)
    {
        var comment = await _db.SocialComments
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.CommentId &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    x.OwnerId == _currentUser.UserId,
                cancellationToken);

        if (comment is null)
            throw new KeyNotFoundException("Comment not found.");

        _db.SocialComments.Remove(comment);

        await _db.SaveChangesAsync(cancellationToken);
    }
}