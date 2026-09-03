// Application/Features/Live/Sessions/PublishStream/PublishStreamResponse.cs

namespace Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;

public sealed record PublishStreamResponse(
    Guid StreamId,
    Guid SessionId,
    string RtmpUrl,
    string PublishToken,
    DateTime PublishTokenExpiresAt);