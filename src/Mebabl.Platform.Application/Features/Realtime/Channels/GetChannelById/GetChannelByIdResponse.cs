namespace Mebabl.Platform.Application.Features.Realtime.Channels.GetChannelById;

public sealed record GetChannelByIdResponse(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt);