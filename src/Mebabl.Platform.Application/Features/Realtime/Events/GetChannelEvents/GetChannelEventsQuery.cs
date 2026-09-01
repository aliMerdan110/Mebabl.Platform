using MediatR;

namespace Mebabl.Platform.Application.Features.Realtime.Events.GetChannelEvents;

public sealed record GetChannelEventsQuery(
    Guid ChannelId,
    int Offset = 0,
    int Limit = 50
) : IRequest<IReadOnlyList<GetChannelEventsResponse>>;