using MediatR;

namespace Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;

public sealed record PublishStreamCommand(
    Guid StreamId
) : IRequest<PublishStreamResponse>;