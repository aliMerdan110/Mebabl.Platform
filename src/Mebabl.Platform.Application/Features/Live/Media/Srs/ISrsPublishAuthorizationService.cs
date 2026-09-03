// Application/Features/Live/Media/Srs/ISrsPublishAuthorizationService.cs

namespace Mebabl.Platform.Application.Features.Live.Media.Srs;

public interface ISrsPublishAuthorizationService
{
    Task<bool> AuthorizePublishAsync(
        SrsPublishRequest request,
        CancellationToken cancellationToken = default);

    Task HandleUnpublishAsync(
        SrsPublishRequest request,
        CancellationToken cancellationToken = default);
}