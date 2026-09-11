using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/applications/{applicationId:guid}/mobile-app-links")]
[Authorize]
public sealed class ApplicationMobileAppLinksController : ControllerBase
{
    private readonly ISender _sender;

    public ApplicationMobileAppLinksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetMobileAppLinksQuery(applicationId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid applicationId,
        [FromBody] CreateMobileAppLinkRequest request,
        CancellationToken cancellationToken)
    {
        var id = await _sender.Send(
            new CreateMobileAppLinkCommand(
                applicationId,
                request.AndroidPackageName,
                request.AndroidSha256CertificateFingerprint,
                request.IosBundleId,
                request.IosTeamId),
            cancellationToken);

        return Ok(new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid applicationId,
        Guid id,
        [FromBody] UpdateMobileAppLinkRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Route id does not match request id.");

        await _sender.Send(
            new UpdateMobileAppLinkCommand(
                applicationId,
                request.Id,
                request.AndroidPackageName,
                request.AndroidSha256CertificateFingerprint,
                request.IosBundleId,
                request.IosTeamId,
                request.IsActive),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid applicationId,
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteMobileAppLinkCommand(
                applicationId,
                id),
            cancellationToken);

        return NoContent();
    }
}

public sealed record CreateMobileAppLinkRequest(
    string? AndroidPackageName,
    string? AndroidSha256CertificateFingerprint,
    string? IosBundleId,
    string? IosTeamId);

public sealed record UpdateMobileAppLinkRequest(
    Guid Id,
    string? AndroidPackageName,
    string? AndroidSha256CertificateFingerprint,
    string? IosBundleId,
    string? IosTeamId,
    bool IsActive);