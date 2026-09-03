// Application/Features/Live/Sessions/StopStream/StopStreamCommand.cs

using MediatR;

namespace Mebabl.Platform.Application.Features.Live.Sessions.StopStream;

public sealed record StopStreamCommand(
    Guid SessionId
) : IRequest;