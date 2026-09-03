using MediatR;

namespace Mebabl.Platform.Application.Features.Live.Streams.CreateStream;

public sealed record CreateStreamCommand(
    Guid ApplicationId,
    string Name,
    string Title,
    string? Description
) : IRequest<CreateStreamResponse>;