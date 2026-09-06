
using MediatR;

namespace Mebabl.Platform.Application.Features.Live.Streams.GetStreams;

public sealed record GetStreamsQuery
    : IRequest<GetStreamsResponse>;
