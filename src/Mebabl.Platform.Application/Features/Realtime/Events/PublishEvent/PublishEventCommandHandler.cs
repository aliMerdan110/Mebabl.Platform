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
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Authenticated application credentials are required.");
        }

        if (request.ChannelId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid channel ID is required.",
                nameof(request.ChannelId));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Event name is required.",
                nameof(request.Name));
        }

        if (request.Payload is null)
        {
            throw new ArgumentException(
                "Event payload is required.",
                nameof(request.Payload));
        }

        var channel = await _context.Channels
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ChannelId &&
                    x.ApplicationId == applicationId &&
                    x.IsActive &&
                    !x.IsDeleted,
                cancellationToken);

        if (channel is null)
        {
            throw new KeyNotFoundException(
                "Active channel not found in this application.");
        }

        var realtimeEvent = new RealtimeEvent
        {
            ChannelId = channel.Id,
            Name = request.Name.Trim(),
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