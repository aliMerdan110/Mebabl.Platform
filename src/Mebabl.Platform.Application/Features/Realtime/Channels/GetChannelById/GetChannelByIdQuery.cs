using MediatR;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.GetChannelById;

public sealed record GetChannelByIdQuery(
    Guid Id
) : IRequest<GetChannelByIdResponse>;