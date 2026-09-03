// Application/Features/Live/Sessions/StartLiveSession/StartLiveSessionCommandHandler.cs

using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Sessions.StartLiveSession;

public sealed class StartLiveSessionCommandHandler
    : IRequestHandler<StartLiveSessionCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IClock _clock;

    public StartLiveSessionCommandHandler(
        IApplicationDbContext dbContext,
        IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task Handle(
        StartLiveSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _dbContext.LiveStreamSessions
            .Include(x => x.LiveStream)
            .FirstOrDefaultAsync(
                x => x.Id == request.SessionId,
                cancellationToken);

        if (session is null)
            throw new KeyNotFoundException(
                "Live stream session was not found.");

        if (session.Status == LiveSessionStatus.Ended)
            throw new InvalidOperationException(
                "The live stream session has already ended.");

        var now = _clock.UtcNow;

        session.Status = LiveSessionStatus.Live;
        session.StartedAt = now;

        session.LiveStream.Status = LiveStreamStatus.Live;
        session.LiveStream.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}