using MediatR;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.CreateChannel;

public sealed record CreateChannelCommand(
    string Name
) : IRequest<CreateChannelResponse>;