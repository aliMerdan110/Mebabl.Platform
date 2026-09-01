using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.SdkAuth.Login;
using Mebabl.Platform.Application.Features.SdkAuth.Register;
using Mebabl.Platform.Application.Features.SdkAuth.Refresh;
using Mebabl.Platform.Application.Features.SdkAuth.Logout;
using Mebabl.Platform.Application.Features.SdkAuth.Me;
using Mebabl.Platform.Application.Features.SdkAuth.ChangePassword;
using Mebabl.Platform.Application.Features.SdkAuth.ForgotPassword;
using Mebabl.Platform.Application.Features.SdkAuth.ResetPassword;


namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/auth")]
public sealed class SdkAuthController : BaseApiController
{

    


    [HttpPost("register")]
    [Authorize(Policy = "Application")]
    public async Task<IActionResult> Register(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

   [HttpPost("login")]
[Authorize(Policy = "Application")]
public async Task<IActionResult> Login(
    LoginUserCommand command,
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        command,
        cancellationToken);

    return Ok(result);
}

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        SdkRefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize(Policy = "Application")]
public async Task<IActionResult> Logout(
    [FromBody] SdkLogoutCommand command,
    CancellationToken cancellationToken)
{
    await Sender.Send(
        command,
        cancellationToken);

    return NoContent();
}

    [Authorize(Policy = "User")]

    [HttpGet("me")]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetCurrentUserQuery(),
            cancellationToken);

        return Ok(result);
    }


  [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] SdkChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("forgot-password")]
    // [Authorize(Policy = "Application")] // قم بإزالتها أو التعليق عليها
    public async Task<IActionResult> ForgotPassword(
        [FromBody] SdkForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    // [Authorize(Policy = "Application")] // قم بإزالتها أو التعليق عليها
    public async Task<IActionResult> ResetPassword(
        [FromBody] SdkResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }



}