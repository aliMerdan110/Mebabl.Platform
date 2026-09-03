// Application/Features/Live/Sessions/StopStream/StopStreamResponse.cs
// لا نحتاج Response body.

namespace Mebabl.Platform.Application.Features.Live.Sessions.StopStream;

public sealed record StopStreamResponse(
    Guid SessionId,
    bool Stopped);