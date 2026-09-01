using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Roles.Create;
using Mebabl.Platform.Application.Features.Roles.GetRoles;
using Mebabl.Platform.Application.Features.Roles.GetRoleById;
using Mebabl.Platform.Application.Features.Roles.Update;
using Mebabl.Platform.Application.Features.Roles.Delete;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public sealed class RolesController : BaseApiController
{
    [HttpPost]
    [ProducesResponseType<CreateRoleResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }


    [HttpGet]
[ProducesResponseType<IReadOnlyList<RoleListItem>>(StatusCodes.Status200OK)]
public async Task<IActionResult> GetAll(
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        new GetRolesQuery(),
        cancellationToken);

    return Ok(result);
}

[HttpGet("{id:guid}")]
[ProducesResponseType<GetRoleByIdResponse>(StatusCodes.Status200OK)]
public async Task<IActionResult> GetById(
    Guid id,
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        new GetRoleByIdQuery(id),
        cancellationToken);

    return Ok(result);
}

[HttpPut("{id:guid}")]
public async Task<IActionResult> Update(
    Guid id,
    UpdateRoleCommand command,
    CancellationToken cancellationToken)
{
    command = command with { Id = id };

    await Sender.Send(command, cancellationToken);

    return NoContent();
}

[HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(
    Guid id,
    CancellationToken cancellationToken)
{
    await Sender.Send(
        new DeleteRoleCommand(id),
        cancellationToken);

    return NoContent();
}


}