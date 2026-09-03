// API/Controllers/LiveController.cs

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Live.Streams.CreateStream;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/applications/{applicationId}/live")]
public sealed class LiveController : ControllerBase
{
    private readonly IMediator _mediator;

    public LiveController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ------------------------------------------------------------
    // CONTROL PLANE
    // Developer -> Application -> LiveStream
    // ------------------------------------------------------------

    [HttpPost("streams")]
    public async Task<IActionResult> CreateStream(
        Guid applicationId,
        [FromBody] CreateStreamRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateStreamCommand(
                applicationId,
                request.Name,
                request.Title,
                request.Description),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record CreateStreamRequest(
    string Name,
    string Title,
    string? Description);
