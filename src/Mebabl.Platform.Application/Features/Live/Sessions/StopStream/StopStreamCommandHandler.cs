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
        // ---------------------------------------------------------
        // Authentication
        // ---------------------------------------------------------

        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        // ---------------------------------------------------------
        // Load Session + Stream
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // Authorization
        //
        // Publisher نفسه يستطيع إيقاف جلسته.
        // live.manage يمكن أن يسمح بإدارة الجلسات لاحقاً.
        // ---------------------------------------------------------

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

        // ---------------------------------------------------------
        // Already Ended
        // ---------------------------------------------------------

        if (session.Status == LiveSessionStatus.Ended)
            return;

        var now = _clock.UtcNow;

        // ---------------------------------------------------------
        // End Session
        //
        // بمجرد Ended يصبح Publish Token غير صالح.
        // ---------------------------------------------------------

        session.Status = LiveSessionStatus.Ended;
        session.EndedAt = now;

        session.LiveStream.Status = LiveStreamStatus.Ended;
        session.LiveStream.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}