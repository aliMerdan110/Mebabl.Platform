using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.ApplicationPlatforms.CreateApplicationPlatform;
using Mebabl.Platform.Application.Features.ApplicationPlatforms.GetApplicationPlatforms;
using Mebabl.Platform.Application.Features.ApplicationPlatforms.GetApplicationPlatformConfig;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize]
[Route("api/applications/{applicationId:guid}/platforms")]
public sealed class ApplicationPlatformsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationPlatformsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ------------------------------------------------------------
    // GET /api/applications/{applicationId}/platforms
    // ------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Get(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetApplicationPlatformsQuery(applicationId),
            cancellationToken);

        return Ok(result);
    }

    // ------------------------------------------------------------
    // POST /api/applications/{applicationId}/platforms
    // ------------------------------------------------------------

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid applicationId,
        [FromBody] CreateApplicationPlatformRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateApplicationPlatformCommand(
            applicationId,
            request.Platform,
            request.Nickname,
            request.PackageName,
            request.BundleId,
            request.Domain);

        var result = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    // ------------------------------------------------------------
    // GET /api/applications/{applicationId}/platforms/{platformId}/config
    // ------------------------------------------------------------

    [HttpGet("{platformId:guid}/config")]
    public async Task<IActionResult> GetConfig(
        Guid applicationId,
        Guid platformId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetApplicationPlatformConfigQuery(
                applicationId,
                platformId),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record CreateApplicationPlatformRequest(
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain
);