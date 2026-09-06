
// Application/Features/Live/Sessions/StopStream/StopStreamCommandHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.Live;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Sessions.StopStream;

public sealed class StopStreamCommandHandler
    : IRequestHandler<StopStreamCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILiveAuthorizationService _authorization;
    private readonly IClock _clock;

    public StopStreamCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILiveAuthorizationService authorization,
        IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _authorization = authorization;
        _clock = clock;
    }

    public async Task Handle(
        StopStreamCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        var session = await _dbContext.LiveStreamSessions
            .Include(x => x.LiveStream)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.SessionId &&
                    x.LiveStream.ApplicationId == applicationId,
                cancellationToken);

        if (session is null)
            throw new KeyNotFoundException(
                "Live stream session was not found.");

        if (session.PublisherUserId != userId)
        {
            var canManage = await _authorization.CanPublishAsync(
                applicationId,
                userId,
                session.LiveStreamId,
                cancellationToken);

            if (!canManage)
                throw new UnauthorizedAccessException(
                    "The current user is not allowed to stop this session.");
        }

        if (session.Status == LiveSessionStatus.Ended)
            return;

        var now = _clock.UtcNow;

        session.Status = LiveSessionStatus.Ended;
        session.EndedAt = now;

        session.LiveStream.Status = LiveStreamStatus.Offline;
        session.LiveStream.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
