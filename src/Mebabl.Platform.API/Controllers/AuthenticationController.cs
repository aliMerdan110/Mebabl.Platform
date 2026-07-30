using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Authentication.RefreshToken;
using Mebabl.Platform.Application.Features.Authentication.DTOs;
using Mebabl.Platform.Application.Features.Authentication.Register;
using Mebabl.Platform.Application.Features.Authentication.Login;
using Microsoft.AspNetCore.Authorization;
using Mebabl.Platform.Application.Features.Authentication.Me;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController : ControllerBase
{
    private readonly ISender _sender;

    public AuthenticationController(ISender sender)
    {
        _sender = sender;
    }


    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }

    [Authorize]
[HttpGet("me")]
[ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<ActionResult<CurrentUserResponse>> Me(
    CancellationToken cancellationToken)
{
    var response = await _sender.Send(
        new GetCurrentUserQuery(),
        cancellationToken);

    return Ok(response);
}


[Authorize]
[HttpGet("claims")]
public IActionResult Claims()
{
    return Ok(User.Claims.Select(c => new
    {
        c.Type,
        c.Value
    }));
}


    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }


    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            command,
            cancellationToken);

        return Ok(response);
    }
}