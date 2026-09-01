using MediatR;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.GetChannels;

public sealed record GetChannelsQuery()
    : IRequest<IReadOnlyList<GetChannelsResponse>>;