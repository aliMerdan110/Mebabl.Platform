namespace Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;

public sealed record PublishStreamResponse(
    Guid StreamId,
    Guid SessionId,
    string WhipUrl,
    string PublishToken,
    DateTime PublishTokenExpiresAt);