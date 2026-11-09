using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reactions.RemoveReaction;

public sealed class RemoveReactionCommandHandler
    : IRequestHandler<RemoveReactionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public RemoveReactionCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(
        RemoveReactionCommand request,
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

        if (request.PostId == Guid.Empty)
        {
            throw new ArgumentException(
                "Post ID is required.",
                nameof(request.PostId));
        }

        var reaction = await _db.SocialReactions
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.PostId == request.PostId &&
                    x.UserId == userId,
                cancellationToken);

        // جعل العملية قابلة للتكرار دون حذف تفاعل مستخدم آخر.
        if (reaction is null)
            return;

        _db.SocialReactions.Remove(reaction);

        await _db.SaveChangesAsync(cancellationToken);
    }
}