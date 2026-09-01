using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.RolePermissions.AssignPermission;
using Mebabl.Platform.Application.Features.RolePermissions.RemovePermission;
using Mebabl.Platform.Application.Features.RolePermissions.GetRolePermissions;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize]
[Route("api/roles/{roleId:guid}/permissions")]
public sealed class RolePermissionsController : BaseApiController
{
    [HttpPost("{permissionId:guid}")]
    public async Task<IActionResult> Assign(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new AssignPermissionToRoleCommand(
                roleId,
                permissionId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{permissionId:guid}")]
    public async Task<IActionResult> Remove(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new RemovePermissionFromRoleCommand(
                roleId,
                permissionId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RolePermissionItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetRolePermissionsQuery(roleId),
            cancellationToken);

        return Ok(result);
    }
}