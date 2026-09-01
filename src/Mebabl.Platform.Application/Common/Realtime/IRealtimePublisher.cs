using System.Text.Json;

namespace Mebabl.Platform.Application.Common.Realtime;

public interface IRealtimePublisher
{
    Task PublishAsync(
        Guid channelId,
        Guid eventId,
        string name,
        JsonDocument payload,
        CancellationToken cancellationToken);
}