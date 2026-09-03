namespace Mebabl.Platform.Application.Features.Live.Streams.CreateStream;

public sealed record CreateStreamResponse(
    Guid Id,
    string Name,
    string Title,
    string StreamKey,
    string RtmpUrl,
    string HlsUrl);