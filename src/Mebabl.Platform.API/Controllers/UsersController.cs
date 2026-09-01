using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Users.GetUsers;
using Mebabl.Platform.Application.Features.Users.GetUserById;
using Mebabl.Platform.Application.Features.Users.Update;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : BaseApiController
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserListItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetUsersQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<GetUserByIdResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetUserByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { Id = id };

        await Sender.Send(command, cancellationToken);

        return NoContent();
    }
}