using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Mebabl.Platform.Application.Features.SdkAuth.Login;
using Mebabl.Platform.Application.Features.SdkAuth.Register;
using Mebabl.Platform.Application.Features.SdkAuth.Refresh;
using Mebabl.Platform.Application.Features.SdkAuth.Logout;
using Mebabl.Platform.Application.Features.SdkAuth.Me;
using Mebabl.Platform.Application.Features.SdkAuth.ChangePassword;
using Mebabl.Platform.Application.Features.SdkAuth.ForgotPassword;
using Mebabl.Platform.Application.Features.SdkAuth.ResetPassword;
using Mebabl.Platform.Application.Features.SdkAuth.VerifyEmail;
using Mebabl.Platform.Application.Features.SdkAuth.ResendVerificationEmail;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/auth")]
[EnableCors("MebablSdk")]
public sealed class SdkAuthController : BaseApiController
{
    [HttpPost("register")]
    [Authorize(Policy = "ApplicationPublic")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    [Authorize(Policy = "ApplicationPublic")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] SdkRefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize(Policy = "ApplicationUser")]
    public async Task<IActionResult> Logout(
        [FromBody] SdkLogoutCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize(Policy = "ApplicationUser")]
    public async Task<IActionResult> Me(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetCurrentUserQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize(Policy = "ApplicationUser")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] SdkChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await Sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [Authorize(Policy = "ApplicationPublic")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] SdkForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] SdkResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] VerifyEmailCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("resend-verification-email")]
    [Authorize(Policy = "ApplicationUser")]
    public async Task<IActionResult> ResendVerificationEmail(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new ResendVerificationEmailCommand(),
            cancellationToken);

        return Ok(result);
    }
}