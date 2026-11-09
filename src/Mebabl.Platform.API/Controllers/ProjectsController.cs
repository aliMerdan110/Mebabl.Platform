using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Projects.CreateProject;
using Mebabl.Platform.Application.Features.Projects.DeleteProject;
using Mebabl.Platform.Application.Features.Projects.DTOs;
using Mebabl.Platform.Application.Features.Projects.GetProject;
using Mebabl.Platform.Application.Features.Projects.GetProjects;
using Mebabl.Platform.Application.Features.Projects.GetProjectSdkConfiguration;
using Mebabl.Platform.Application.Features.Projects.UpdateProject;

using Mebabl.Platform.Application.Features.Projects.Platforms.CreateProjectPlatform;
using Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatform;
using Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatforms;
using Mebabl.Platform.Application.Features.Projects.Platforms.UpdateProjectPlatform;
using Mebabl.Platform.Application.Features.Projects.Platforms.DeleteProjectPlatform;
using Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatformConfig;
using Mebabl.Platform.Application.Features.Projects.Platforms.Fingerprints;



namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/developers/projects")]
[Authorize(Policy = "Developer")]
public sealed class ProjectsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new { projectId = result.Id },
            result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProjectsQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{projectId:guid}")]
    public async Task<IActionResult> Get(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProjectQuery(projectId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{projectId:guid}")]
    public async Task<IActionResult> Update(
        Guid projectId,
        [FromBody] UpdateProjectCommand command,
        CancellationToken cancellationToken)
    {
        if (projectId != command.ProjectId)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteProjectCommand(projectId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{projectId:guid}/sdk-configuration")]
    [ProducesResponseType(typeof(ProjectSdkConfigurationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectSdkConfigurationDto>> GetSdkConfiguration(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProjectSdkConfigurationQuery(projectId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{projectId:guid}/sdk-configuration/download")]
    [Produces("application/json")]
    public async Task<IActionResult> DownloadSdkConfiguration(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProjectSdkConfigurationQuery(projectId),
            cancellationToken);

        var json = JsonSerializer.Serialize(result, JsonOptions);

        return File(
            System.Text.Encoding.UTF8.GetBytes(json),
            "application/json",
            "mebabl-services.json");
    }


   
   
    [HttpPost("{projectId:guid}/platforms")]
    public async Task<IActionResult> CreatePlatform(
        Guid projectId,
        [FromBody] CreateProjectPlatformRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectPlatformCommand(
            projectId,
            request.Platform,
            request.Nickname,
            request.PackageName,
            request.BundleId,
            request.Domain);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{projectId:guid}/platforms")]
    public async Task<IActionResult> GetPlatforms(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProjectPlatformsQuery(projectId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{projectId:guid}/platforms/{platformId:guid}")]
    public async Task<IActionResult> GetPlatform(
        Guid projectId,
        Guid platformId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProjectPlatformQuery(projectId, platformId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{projectId:guid}/platforms/{platformId:guid}")]
    public async Task<IActionResult> UpdatePlatform(
        Guid projectId,
        Guid platformId,
        [FromBody] UpdateProjectPlatformRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProjectPlatformCommand(
            projectId,
            platformId,
            request.Platform,
            request.Nickname,
            request.PackageName,
            request.BundleId,
            request.Domain,
            request.IsActive);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{projectId:guid}/platforms/{platformId:guid}")]
    public async Task<IActionResult> DeletePlatform(
        Guid projectId,
        Guid platformId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteProjectPlatformCommand(projectId, platformId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{projectId:guid}/platforms/{platformId:guid}/config")]
    public async Task<IActionResult> GetPlatformConfig(
        Guid projectId,
        Guid platformId,
        [FromQuery] Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProjectPlatformConfigQuery(
                projectId,
                platformId,
                applicationId),
            cancellationToken);

        return Ok(result);
    }


[HttpGet("{projectId:guid}/platforms/{platformId:guid}/fingerprints")]
[ProducesResponseType(
    typeof(IReadOnlyList<ProjectPlatformFingerprintDto>),
    StatusCodes.Status200OK)]
public async Task<IActionResult> GetPlatformFingerprints(
    Guid projectId,
    Guid platformId,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(
        new GetProjectPlatformFingerprintsQuery(projectId, platformId),
        cancellationToken);

    return Ok(result);
}

[HttpPost("{projectId:guid}/platforms/{platformId:guid}/fingerprints")]
[ProducesResponseType(
    typeof(ProjectPlatformFingerprintDto),
    StatusCodes.Status200OK)]
public async Task<IActionResult> CreatePlatformFingerprint(
    Guid projectId,
    Guid platformId,
    [FromBody] CreateProjectPlatformFingerprintRequest request,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(
        new CreateProjectPlatformFingerprintCommand(
            projectId,
            platformId,
            request.FingerprintType,
            request.Value),
        cancellationToken);

    return Ok(result);
}

[HttpDelete(
    "{projectId:guid}/platforms/{platformId:guid}/fingerprints/{fingerprintId:guid}")]
public async Task<IActionResult> DeletePlatformFingerprint(
    Guid projectId,
    Guid platformId,
    Guid fingerprintId,
    CancellationToken cancellationToken)
{
    await _mediator.Send(
        new DeleteProjectPlatformFingerprintCommand(
            projectId,
            platformId,
            fingerprintId),
        cancellationToken);

    return NoContent();
}


}


public sealed record CreateProjectPlatformRequest(
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain);

public sealed record UpdateProjectPlatformRequest(
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain,
    bool IsActive);


public sealed record CreateProjectPlatformFingerprintRequest(
    string FingerprintType,
    string Value);




