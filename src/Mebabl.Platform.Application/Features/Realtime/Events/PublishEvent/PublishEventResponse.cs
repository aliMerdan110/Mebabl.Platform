using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Realtime.Events.PublishEvent;

public sealed record PublishEventResponse(
    Guid Id,
    Guid ChannelId,
    string Name,
    JsonDocument Payload,
    DateTime CreatedAt);