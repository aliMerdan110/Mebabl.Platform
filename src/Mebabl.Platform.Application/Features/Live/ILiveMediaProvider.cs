namespace Mebabl.Platform.Application.Features.Live;

public interface ILiveMediaProvider
{
    Task<bool> ValidatePublishAsync(
        Guid applicationId,
        string streamKey,
        CancellationToken cancellationToken = default);

    Task<MediaStreamStatus> GetStatusAsync(
        string streamName,
        CancellationToken cancellationToken = default);

    Task<PlaybackInfo> GetPlaybackAsync(
        string streamName,
        CancellationToken cancellationToken = default);
}

public sealed record MediaStreamStatus(
    bool IsLive);

public sealed record PlaybackInfo(
    string HlsUrl);