// API/Controllers/SrsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Live.Media.Srs;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/internal/srs")]
public sealed class SrsController : ControllerBase
{
    private readonly ISrsPublishAuthorizationService _authorization;

    public SrsController(
        ISrsPublishAuthorizationService authorization)
    {
        _authorization = authorization;
    }

    // ------------------------------------------------------------
    // SRS -> Mebabl
    //
    // Called before SRS accepts an RTMP publisher.
    // ------------------------------------------------------------

    [HttpPost("on-publish")]
    public async Task<IActionResult> OnPublish(
        [FromForm] SrsPublishRequest request,
        CancellationToken cancellationToken)
    {
        var allowed =
            await _authorization.AuthorizePublishAsync(
                request,
                cancellationToken);

        if (!allowed)
            return StatusCode(StatusCodes.Status403Forbidden);

        return Ok();
    }

    // ------------------------------------------------------------
    // SRS -> Mebabl
    //
    // Called when publisher disconnects.
    // ------------------------------------------------------------

    [HttpPost("on-unpublish")]
    public async Task<IActionResult> OnUnpublish(
        [FromForm] SrsPublishRequest request,
        CancellationToken cancellationToken)
    {
        await _authorization.HandleUnpublishAsync(
            request,
            cancellationToken);

        return Ok();
    }
}