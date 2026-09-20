using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;
using Mebabl.Platform.Application.Features.Live.Sessions.StopStream;
using Mebabl.Platform.Application.Features.Live.Streams.GetStreams;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize(Policy = "ApplicationUser")]
[Route("api/sdk/live")]
public sealed class SdkLiveController : ControllerBase
{
    private readonly ISender _sender;

    public SdkLiveController(ISender sender)
    {
        _sender = sender;
    }

    // =========================================================
    // Streams
    // =========================================================

    [HttpGet("streams")]
    public async Task<IActionResult> GetStreams(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetStreamsQuery(),
            cancellationToken);

        return Ok(result);
    }

    // =========================================================
    // Publish
    // =========================================================

    [HttpPost("publish")]
    public async Task<IActionResult> Publish(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new PublishStreamCommand(),
            cancellationToken);

        return Ok(result);
    }

    // =========================================================
    // Stop
    // =========================================================

    [HttpPost("stop")]
    public async Task<IActionResult> Stop(
        [FromBody] StopStreamRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new StopStreamCommand(
                request.SessionId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record StopStreamRequest(
    Guid SessionId);