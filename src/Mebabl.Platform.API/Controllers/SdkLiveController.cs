using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Live.Sessions.PublishStream;
using Mebabl.Platform.Application.Features.Live.Sessions.StopStream;
using Mebabl.Platform.Application.Features.Live.Streams.GetStreams;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/live")]
[Authorize(Policy = "ApplicationUser")]
[EnableCors("MebablSdk")]
public sealed class SdkLiveController : ControllerBase
{
    private readonly IMediator _mediator;

    public SdkLiveController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("streams")]
    public async Task<IActionResult> GetStreams(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetStreamsQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("publish")]
public async Task<IActionResult> Publish(
    [FromBody] PublishStreamRequest? request,
    CancellationToken cancellationToken)
{
    if (request is null || request.StreamId == Guid.Empty)
    {
        return BadRequest(new
        {
            message = "A valid stream ID is required."
        });
    }

    var result = await _mediator.Send(
        new PublishStreamCommand(request.StreamId),
        cancellationToken);

    return Ok(result);
}

    [HttpPost("stop")]
    public async Task<IActionResult> Stop(
        [FromBody] StopStreamRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.SessionId == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "A valid session ID is required."
            });
        }

        await _mediator.Send(
            new StopStreamCommand(request.SessionId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record StopStreamRequest(Guid SessionId);

public sealed record PublishStreamRequest(Guid StreamId);