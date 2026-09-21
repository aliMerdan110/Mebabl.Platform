using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.ApplicationAuthentication.Login;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.Providers;
using Mebabl.Platform.Application.Features.Applications.Users.CreateApplicationUser;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api")]
public sealed class ApplicationAuthController : BaseApiController
{
    // يفصل بين مصادقة التطبيق وإدارة إعدادات ومستخدمي التطبيق.
    [AllowAnonymous]
    [HttpPost("application-auth/token")]
    public async Task<IActionResult> Token(
        [FromBody] ApplicationLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Policy = "Developer")]
    [HttpGet(
        "applications/{applicationId:guid}/authentication/providers")]
    public async Task<IActionResult> GetProviders(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetAuthProvidersQuery(applicationId),
            cancellationToken);

        return Ok(result);
    }

    [Authorize(Policy = "Developer")]
    [HttpPut(
        "applications/{applicationId:guid}/authentication/providers/{provider}")]
    public async Task<IActionResult> ToggleProvider(
        Guid applicationId,
        string provider,
        [FromBody] ToggleAuthProviderRequest request,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new ToggleAuthProviderCommand(
                applicationId,
                provider,
                request.IsEnabled),
            cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = "Developer")]
    [HttpPost(
        "applications/{applicationId:guid}/users")]
    public async Task<IActionResult> CreateUser(
        Guid applicationId,
        [FromBody] CreateApplicationUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new CreateApplicationUserCommand(
                applicationId,
                request.Email,
                request.Password,
                request.Username,
                request.DisplayName),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}

public sealed record ToggleAuthProviderRequest(
    bool IsEnabled);

public sealed record CreateApplicationUserRequest(
    string Email,
    string Password,
    string Username,
    string DisplayName);