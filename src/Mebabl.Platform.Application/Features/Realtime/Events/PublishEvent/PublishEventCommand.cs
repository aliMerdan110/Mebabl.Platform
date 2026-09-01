using System.Text.Json;
using MediatR;

namespace Mebabl.Platform.Application.Features.Realtime.Events.PublishEvent;

public sealed record PublishEventCommand(
    Guid ChannelId,
    string Name,
    JsonDocument Payload
) : IRequest<PublishEventResponse>;