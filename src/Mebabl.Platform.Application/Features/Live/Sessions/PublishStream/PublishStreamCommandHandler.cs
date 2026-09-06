using System.Security.Cryptography;
using System.Text;
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
        // Application-defined Authorization
        // ---------------------------------------------------------

        var allowed = await _authorization.CanPublishAsync(
    applicationId,
    userId,
    cancellationToken);

        if (!allowed)
            throw new UnauthorizedAccessException(
                "The current user is not allowed to publish.");

        // ---------------------------------------------------------
        // Find an available stream
        // ---------------------------------------------------------

        var stream = await _dbContext.LiveStreams
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.Status != LiveStreamStatus.Live,
                cancellationToken);

        // ---------------------------------------------------------
        // Create stream automatically when none exists
        // ---------------------------------------------------------

        if (stream is null)
        {
            var nowForStream = _clock.UtcNow;

            stream = new LiveStream
            {
                Id = Guid.NewGuid(),

                ApplicationId = applicationId,

                Name = $"live-{userId:N}",

                Title = "Live Stream",

                Description = null,

                Status = LiveStreamStatus.Offline,

                CreatedAt = nowForStream,

                UpdatedAt = nowForStream
            };

            var rawStreamKey = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));

            var keyHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(rawStreamKey)));

            stream.Credentials.Add(
                new StreamCredential
                {
                    Id = Guid.NewGuid(),

                    LiveStreamId = stream.Id,

                    KeyHash = keyHash,

                    IsActive = true,

                    CreatedAt = nowForStream
                });

            _dbContext.LiveStreams.Add(stream);
        }

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

        // ---------------------------------------------------------
        // Publish Token Lifetime
        // ---------------------------------------------------------

        var now = _clock.UtcNow;

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
        // WHIP URL
        // ---------------------------------------------------------

        var whipUrl =
            $"https://live.mebabl.com/rtc/v1/whip/" +
            $"?app=live" +
            $"&stream=livestream" +
            $"&sessionId={session.Id}" +
            $"&token={Uri.EscapeDataString(rawPublishToken)}";

        return new PublishStreamResponse(
            stream.Id,
            session.Id,
            whipUrl,
            rawPublishToken,
            publishTokenExpiresAt);
    }
}
