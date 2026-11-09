
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.Live;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;

public sealed class PublishStreamCommandHandler
    : IRequestHandler<PublishStreamCommand, PublishStreamResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILiveAuthorizationService _authorization;
    private readonly IPublishTokenService _publishTokenService;
    private readonly IClock _clock;

    public PublishStreamCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILiveAuthorizationService authorization,
        IPublishTokenService publishTokenService,
        IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _authorization = authorization;
        _publishTokenService = publishTokenService;
        _clock = clock;
    }

    public async Task<PublishStreamResponse> Handle(
        PublishStreamCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        if (!_currentUser.IsAuthenticated ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        if (request.StreamId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid stream ID is required.",
                nameof(request.StreamId));
        }

        var allowed = await _authorization.CanPublishAsync(
            applicationId,
            userId,
            cancellationToken);

        if (!allowed)
        {
            throw new UnauthorizedAccessException(
                "The current user is not allowed to publish.");
        }

        // لا تسمح بالنشر على بث تابع لتطبيق آخر.
        var stream = await _dbContext.LiveStreams
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.StreamId &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (stream is null)
        {
            throw new KeyNotFoundException(
                "Live stream was not found.");
        }

        var activeSession = await _dbContext.LiveStreamSessions
            .FirstOrDefaultAsync(
                x =>
                    x.LiveStreamId == stream.Id &&
                    x.Status != LiveSessionStatus.Ended,
                cancellationToken);

        if (activeSession is not null)
        {
            var checkedAt = _clock.UtcNow;

            var canExpire =
                activeSession.Status == LiveSessionStatus.Starting &&
                checkedAt >= activeSession.PublishTokenExpiresAt;

            if (!canExpire)
            {
                throw new InvalidOperationException(
                    "The live stream already has an active session.");
            }

            // انتهت صلاحية محاولة النشر ولم يبدأ البث.
            activeSession.Status = LiveSessionStatus.Ended;
            activeSession.EndedAt = checkedAt;

            stream.Status = LiveStreamStatus.Offline;
            stream.UpdatedAt = checkedAt;

            // حفظ الإنهاء قبل إنشاء جلسة جديدة،
            // لأن PostgreSQL يمنع الجلسات غير المنتهية المكررة.
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var rawPublishToken =
            _publishTokenService.GenerateToken();

        var publishTokenHash =
            _publishTokenService.HashToken(rawPublishToken);

        var now = _clock.UtcNow;
        var expiresAt = now.AddMinutes(15);

        var session =
            new Mebabl.Platform.Domain.Live.LiveStreamSession
            {
                Id = Guid.NewGuid(),
                LiveStreamId = stream.Id,
                PublisherUserId = userId,
                PublishTokenHash = publishTokenHash,
                PublishTokenExpiresAt = expiresAt,
                Status = LiveSessionStatus.Starting,
                CreatedAt = now,
                StartedAt = null,
                EndedAt = null
            };

        stream.Status = LiveStreamStatus.Starting;
        stream.UpdatedAt = now;

        _dbContext.LiveStreamSessions.Add(session);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // تحقّق من وجود جلسة أخرى نشطة بعد تعارض الحفظ.
            var hasActiveSession =
                await _dbContext.LiveStreamSessions.AnyAsync(
                    x =>
                        x.LiveStreamId == stream.Id &&
                        x.Status != LiveSessionStatus.Ended,
                    cancellationToken);

            if (hasActiveSession)
            {
                throw new InvalidOperationException(
                    "The live stream already has an active session.");
            }

            // لا نخفي أخطاء قاعدة البيانات الأخرى.
            throw;
        }

        var whipUrl =
            "https://live.mebabl.com/rtc/v1/whip/" +
            $"?app=live" +
            $"&stream={Uri.EscapeDataString(stream.Name)}" +
            $"&sessionId={session.Id}" +
            $"&token={Uri.EscapeDataString(rawPublishToken)}";

        return new PublishStreamResponse(
            stream.Id,
            session.Id,
            whipUrl,
            rawPublishToken,
            expiresAt);
    }
}
