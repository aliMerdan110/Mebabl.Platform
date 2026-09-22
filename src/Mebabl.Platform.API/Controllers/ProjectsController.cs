
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Projects.CreateProject;
using Mebabl.Platform.Application.Features.Projects.DeleteProject;
using Mebabl.Platform.Application.Features.Projects.GetProject;
using Mebabl.Platform.Application.Features.Projects.GetProjects;
using Mebabl.Platform.Application.Features.Projects.UpdateProject;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/developers/projects")]
[Authorize(Policy = "Developer")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateProjectCommand command,
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
        UpdateProjectCommand command,
        CancellationToken cancellationToken)
    {
        if (projectId != command.ProjectId)
            return BadRequest();

        var result = await _mediator.Send(
            command,
            cancellationToken);

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
}
