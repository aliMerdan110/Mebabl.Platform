namespace Mebabl.Platform.Application.Features.Realtime.Channels.CreateChannel;

public sealed record CreateChannelResponse(
    Guid Id,
    string Name);