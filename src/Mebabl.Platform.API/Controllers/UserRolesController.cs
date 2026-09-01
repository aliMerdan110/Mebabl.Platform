using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.UserRoles.AssignRole;
using Mebabl.Platform.Application.Features.UserRoles.RemoveRole;
using Mebabl.Platform.Application.Features.UserRoles.GetUserRoles;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:guid}/roles")]
public sealed class UserRolesController : BaseApiController
{
    [HttpPost("{roleId:guid}")]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new AssignRoleToUserCommand(
                userId,
                roleId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new RemoveRoleFromUserCommand(
                userId,
                roleId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserRoleItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetUserRolesQuery(userId),
            cancellationToken);

        return Ok(result);
    }
}