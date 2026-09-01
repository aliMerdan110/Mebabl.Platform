namespace Mebabl.Platform.Application.Features.Realtime.Channels.GetChannels;

public sealed record GetChannelsResponse(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt);