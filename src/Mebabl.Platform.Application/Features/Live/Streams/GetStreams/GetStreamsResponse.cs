
namespace Mebabl.Platform.Application.Features.Live.Streams.GetStreams;

public sealed record GetStreamsResponse(
    IReadOnlyList<GetStreamsItem> Items);

public sealed record GetStreamsItem(
    Guid Id,
    string Name,
    string Title,
    string? Description,
    string PlaybackUrl,
    string Status);
