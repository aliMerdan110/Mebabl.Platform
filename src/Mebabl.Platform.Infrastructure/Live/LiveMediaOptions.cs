namespace Mebabl.Platform.Infrastructure.Live;

public sealed class LiveMediaOptions
{
    public string RtmpBaseUrl { get; set; }
        = "rtmp://live.mebabl.com/live";

    public string HlsBaseUrl { get; set; }
        = "https://live.mebabl.com/hls";

    public string SrsApiUrl { get; set; }
        = "http://localhost:1985";
}