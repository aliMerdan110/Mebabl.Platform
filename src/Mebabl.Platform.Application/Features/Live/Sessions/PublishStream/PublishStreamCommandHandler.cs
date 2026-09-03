// Application/Features/Live/Sessions/PublishStream/PublishStreamCommandHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.Live;
using Mebabl.Platform.Domain.Live;
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
        // ---------------------------------------------------------
        // Application User Authentication
        // ---------------------------------------------------------

        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        // ---------------------------------------------------------
        // Load Stream
        // ---------------------------------------------------------

        var stream = await _dbContext.LiveStreams
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.StreamId &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (stream is null)
            throw new KeyNotFoundException(
                "Live stream was not found.");

        // ---------------------------------------------------------
        // Application-defined Authorization
        // ---------------------------------------------------------

        var allowed = await _authorization.CanPublishAsync(
            applicationId,
            userId,
            stream.Id,
            cancellationToken);

        if (!allowed)
            throw new UnauthorizedAccessException(
                "The current user is not allowed to publish to this stream.");

        // ---------------------------------------------------------
        // Active Session
        // ---------------------------------------------------------

        var activeSession = await _dbContext.LiveStreamSessions
            .FirstOrDefaultAsync(
                x =>
                    x.LiveStreamId == stream.Id &&
                    x.Status != LiveSessionStatus.Ended,
                cancellationToken);

        if (activeSession is not null)
            throw new InvalidOperationException(
                "The live stream already has an active session.");

        // ---------------------------------------------------------
        // Active Stream Credential
        // ---------------------------------------------------------

        var credentialExists = await _dbContext.StreamCredentials
            .AnyAsync(
                x =>
                    x.LiveStreamId == stream.Id &&
                    x.IsActive,
                cancellationToken);

        if (!credentialExists)
            throw new InvalidOperationException(
                "No active stream credential exists.");

        // ---------------------------------------------------------
        // Generate Publish Token
        // ---------------------------------------------------------

        var rawPublishToken =
            _publishTokenService.GenerateToken();

        var publishTokenHash =
            _publishTokenService.HashToken(
                rawPublishToken);

        var now = _clock.UtcNow;

        // ---------------------------------------------------------
        // Publish Token Lifetime
        // ---------------------------------------------------------

        var publishTokenExpiresAt =
            now.AddMinutes(15);

        // ---------------------------------------------------------
        // Create Session
        // ---------------------------------------------------------

        var session = new LiveStreamSession
        {
            Id = Guid.NewGuid(),

            LiveStreamId = stream.Id,

            PublisherUserId = userId,

            PublishTokenHash = publishTokenHash,

            PublishTokenExpiresAt = publishTokenExpiresAt,

            Status = LiveSessionStatus.Starting,

            CreatedAt = now,

            StartedAt = null,

            EndedAt = null
        };

        stream.Status = LiveStreamStatus.Starting;
        stream.UpdatedAt = now;

        _dbContext.LiveStreamSessions.Add(session);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        // ---------------------------------------------------------
        // Return RAW Token
        //
        // لا يتم إعادة Hash للمستخدم.
        // ---------------------------------------------------------

        return new PublishStreamResponse(
            stream.Id,
            session.Id,
            "rtmp://live.mebabl.com/live",
            rawPublishToken,
            publishTokenExpiresAt);
    }
}