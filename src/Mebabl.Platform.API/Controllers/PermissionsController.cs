using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Permissions.Create;
using Mebabl.Platform.Application.Features.Permissions.GetPermissions;
using Mebabl.Platform.Application.Features.Permissions.GetPermissionById;
using Mebabl.Platform.Application.Features.Permissions.Update;
using Mebabl.Platform.Application.Features.Permissions.Delete;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize]
public sealed class PermissionsController : BaseApiController
{
    [HttpPost]
    [ProducesResponseType<CreatePermissionResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        CreatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PermissionListItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetPermissionsQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetPermissionByIdResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetPermissionByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { Id = id };

        await Sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new DeletePermissionCommand(id),
            cancellationToken);

        return NoContent();
    }
}