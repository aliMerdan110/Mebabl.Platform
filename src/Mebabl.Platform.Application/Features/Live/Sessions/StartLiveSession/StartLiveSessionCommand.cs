// Application/Features/Live/Sessions/StartLiveSession/StartLiveSessionCommand.cs

using MediatR;

namespace Mebabl.Platform.Application.Features.Live.Sessions.StartLiveSession;

public sealed record StartLiveSessionCommand(
    Guid SessionId
) : IRequest;