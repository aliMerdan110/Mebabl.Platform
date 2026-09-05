using System.Text.Json.Serialization;

namespace Mebabl.Platform.Application.Features.Live.Media.Srs;

public sealed record SrsPublishRequest(
    string Action,
    [property: JsonPropertyName("client_id")] string ClientId,
    string Ip,
    string Vhost,
    string App,
    string Stream,
    string Param);