// Application/Features/Live/Sessions/PublishStream/PublishStreamCommand.cs

using MediatR;

namespace Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;

public sealed record PublishStreamCommand(
    Guid StreamId
) : IRequest<PublishStreamResponse>;

