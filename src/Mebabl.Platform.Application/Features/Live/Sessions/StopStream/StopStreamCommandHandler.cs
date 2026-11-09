using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Sessions.StopStream;

public sealed class StopStreamCommandHandler
    : IRequestHandler<StopStreamCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public StopStreamCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task Handle(
        StopStreamCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty ||
            _currentUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The current user is not authenticated.");
        }

        if (request.SessionId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid session ID is required.",
                nameof(request.SessionId));
        }

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
        {
            throw new KeyNotFoundException(
                "Live stream session was not found.");
        }

        // لا يجوز إيقاف جلسة مستخدم آخر.
        if (session.PublisherUserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You can only stop your own live session.");
        }

        if (session.Status == LiveSessionStatus.Ended)
            return;

        var now = _clock.UtcNow;

        session.Status = LiveSessionStatus.Ended;
        session.EndedAt = now;

        // لا تحول البث إلى Offline إذا كانت له جلسة نشطة أخرى.
        var hasOtherActiveSession =
            await _dbContext.LiveStreamSessions.AnyAsync(
                x =>
                    x.LiveStreamId == session.LiveStreamId &&
                    x.Id != session.Id &&
                    x.Status != LiveSessionStatus.Ended,
                cancellationToken);

        if (!hasOtherActiveSession)
        {
            session.LiveStream.Status = LiveStreamStatus.Offline;
            session.LiveStream.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}