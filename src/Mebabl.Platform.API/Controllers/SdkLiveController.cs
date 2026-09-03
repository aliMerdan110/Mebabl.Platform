// API/Controllers/SdkLiveController.cs

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;
using Mebabl.Platform.Application.Features.Live.Sessions.StopStream;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/live")]
[Authorize]
public sealed class SdkLiveController : ControllerBase
{
    private readonly IMediator _mediator;

    public SdkLiveController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ------------------------------------------------------------
    // Start Publish Session
    // ------------------------------------------------------------

    [HttpPost("publish")]
    public async Task<IActionResult> Publish(
        [FromBody] PublishStreamRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new PublishStreamCommand(
                request.StreamId),
            cancellationToken);

        return Ok(result);
    }

    // ------------------------------------------------------------
    // Stop Publish Session
    // ------------------------------------------------------------

    [HttpPost("stop")]
    public async Task<IActionResult> Stop(
        [FromBody] StopStreamRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new StopStreamCommand(
                request.SessionId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record PublishStreamRequest(
    Guid StreamId);

public sealed record StopStreamRequest(
    Guid SessionId);