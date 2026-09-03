// Application/Features/Live/Sessions/StartLiveSession/StartLiveSessionResponse.cs

namespace Mebabl.Platform.Application.Features.Live.Sessions.StartLiveSession;

public sealed record StartLiveSessionResponse(
    Guid SessionId,
    bool Live);