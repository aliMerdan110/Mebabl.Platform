using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Developers.ForgotPassword;
using Mebabl.Platform.Application.Features.Developers.Login;
using Mebabl.Platform.Application.Features.Developers.Logout;
using Mebabl.Platform.Application.Features.Developers.Me;
using Mebabl.Platform.Application.Features.Developers.RefreshToken;
using Mebabl.Platform.Application.Features.Developers.Register;
using Mebabl.Platform.Application.Features.Developers.ResetPassword;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/developers")]
public sealed class DevelopersController : BaseApiController
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<RegisterDeveloperResponse>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Register(
        RegisterDeveloperCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginDeveloperResponse>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        LoginDeveloperCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType<RefreshDeveloperTokenResponse>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken(
        RefreshDeveloperTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Policy = "Developer")]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetCurrentDeveloperQuery(),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Policy = "Developer")]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        LogoutDeveloperCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType<ForgotPasswordResponse>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType<ResetPasswordResponse>(
        StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }
}