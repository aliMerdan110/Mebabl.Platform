using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Realtime;

namespace Mebabl.Platform.Application.Features.Realtime.Events.PublishEvent;

public sealed class PublishEventCommandHandler
    : IRequestHandler<PublishEventCommand, PublishEventResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public PublishEventCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<PublishEventResponse> Handle(
        PublishEventCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var channel = await _context.Channels
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ChannelId &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    x.IsActive &&
                    !x.IsDeleted,
                cancellationToken);

        if (channel is null)
            throw new Exception("Channel not found.");

        var realtimeEvent = new RealtimeEvent
        {
            ChannelId = channel.Id,
            Name = request.Name,
            Payload = request.Payload
        };

        _context.RealtimeEvents.Add(realtimeEvent);

        await _context.SaveChangesAsync(cancellationToken);

        return new PublishEventResponse(
            realtimeEvent.Id,
            realtimeEvent.ChannelId,
            realtimeEvent.Name,
            realtimeEvent.Payload,
            realtimeEvent.CreatedAt);
    }
}