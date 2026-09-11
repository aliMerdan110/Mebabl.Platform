using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/applications/mobile-app-links")]
[Authorize(Policy = "Application")]
public sealed class ApplicationMobileAppLinksController : ControllerBase
{
    private readonly ISender _sender;

    public ApplicationMobileAppLinksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMobileAppLinksQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateMobileAppLinkCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);

        return Ok(new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateMobileAppLinkCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match request id.");

        await _sender.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteMobileAppLinkCommand(id),
            cancellationToken);

        return NoContent();
    }
}