using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reactions.RemoveReaction;

public sealed class RemoveReactionCommandHandler
    : IRequestHandler<RemoveReactionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public RemoveReactionCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        RemoveReactionCommand request,
        CancellationToken cancellationToken)
    {
        var reaction = await _db.SocialReactions
            .FirstOrDefaultAsync(
                x => x.ApplicationId == _currentApplication.ApplicationId &&
                     x.PostId == request.PostId &&
                     x.UserId == _currentUser.UserId,
                cancellationToken);

        if (reaction is null)
            return;

        _db.SocialReactions.Remove(reaction);

        await _db.SaveChangesAsync(cancellationToken);
    }
}