using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Realtime.Events.GetChannelEvents;

public sealed record GetChannelEventsResponse(
    Guid Id,
    Guid ChannelId,
    string Name,
    JsonDocument Payload,
    DateTime CreatedAt);