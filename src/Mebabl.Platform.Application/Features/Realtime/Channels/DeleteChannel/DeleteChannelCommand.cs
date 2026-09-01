using MediatR;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.DeleteChannel;

public sealed record DeleteChannelCommand(
    Guid Id
) : IRequest;