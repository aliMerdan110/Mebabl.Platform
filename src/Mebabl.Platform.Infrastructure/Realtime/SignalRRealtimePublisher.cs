using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Mebabl.Platform.Application.Common.Realtime;

namespace Mebabl.Platform.Infrastructure.Realtime;

public sealed class SignalRRealtimePublisher
    : IRealtimePublisher
{
    private readonly IHubContext<RealtimeHub> _hubContext;

    public SignalRRealtimePublisher(
        IHubContext<RealtimeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishAsync(
        Guid channelId,
        Guid eventId,
        string name,
        JsonDocument payload,
        CancellationToken cancellationToken)
    {
        await _hubContext.Clients
            .Group(channelId.ToString())
            .SendAsync(
                "eventReceived",
                new
                {
                    id = eventId,
                    channelId,
                    name,
                    payload
                },
                cancellationToken);
    }
}