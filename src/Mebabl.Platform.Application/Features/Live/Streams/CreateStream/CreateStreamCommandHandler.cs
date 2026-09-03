using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Live;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Streams.CreateStream;

public sealed class CreateStreamCommandHandler
    : IRequestHandler<CreateStreamCommand, CreateStreamResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;
    private readonly IClock _clock;

    public CreateStreamCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper,
        IClock clock)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
        _clock = clock;
    }

    public async Task<CreateStreamResponse> Handle(
        CreateStreamCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (application is null)
            throw new UnauthorizedAccessException(
                "Application does not belong to the current developer.");

        var name = request.Name.Trim();

        var exists = await _dbContext.LiveStreams
            .AnyAsync(
                x =>
                    x.ApplicationId == application.Id &&
                    x.Name == name,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                "A live stream with this name already exists.");

        var rawStreamKey = GenerateStreamKey();

        var keyHash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(rawStreamKey)));

        var now = _clock.UtcNow;

        var stream = new LiveStream
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            Name = name,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Status = LiveStreamStatus.Offline,
            CreatedAt = now,
            UpdatedAt = now
        };

        var credential = new StreamCredential
        {
            Id = Guid.NewGuid(),
            LiveStreamId = stream.Id,
            KeyHash = keyHash,
            IsActive = true,
            CreatedAt = now
        };

        stream.Credentials.Add(credential);

        _dbContext.LiveStreams.Add(stream);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateStreamResponse(
            stream.Id,
            stream.Name,
            stream.Title,
            rawStreamKey,
            "rtmp://live.mebabl.com/live",
            $"https://live.mebabl.com/hls/{stream.Id}.m3u8");
    }

    private static string GenerateStreamKey()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }
}