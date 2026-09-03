// Application/Features/Live/Media/Srs/SrsPublishRequest.cs

namespace Mebabl.Platform.Application.Features.Live.Media.Srs;

public sealed record SrsPublishRequest(
    string Action,
    string ClientId,
    string Ip,
    string Vhost,
    string App,
    string Stream,
    string Param);