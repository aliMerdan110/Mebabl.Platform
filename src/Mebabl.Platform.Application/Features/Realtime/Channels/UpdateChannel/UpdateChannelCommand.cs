using MediatR;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.UpdateChannel;

public sealed record UpdateChannelCommand(
    Guid Id,
    string Name,
    bool IsActive
) : IRequest;