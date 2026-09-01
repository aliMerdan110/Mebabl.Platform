using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.Login;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.Providers;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.Settings;
using Mebabl.Platform.Application.Features.Applications.Users.CreateApplicationUser;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api")]
public class ApplicationAuthController : BaseApiController
{

    
    // ------------------------------------------------------------
    // Application User Authentication
    // ------------------------------------------------------------

    // POST /api/application-auth/token
    //
    // Used by an application user to sign in.
    //
    [HttpPost("application-auth/token")]
    public async Task<IActionResult> Token(
        ApplicationLoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }


    // ------------------------------------------------------------
    // Authentication Providers
    // Developer Console
    // ------------------------------------------------------------

    // GET /api/applications/{applicationId}/authentication/providers
    //
    // Returns authentication providers belonging
    // to the selected application.
    //
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


    // PUT /api/applications/{applicationId}/authentication/providers/{provider}
    //
    // Enable or disable an authentication provider
    // for the selected application.
    //
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


    // ------------------------------------------------------------
// Application Users - Developer Console
// ------------------------------------------------------------

// POST /api/applications/{applicationId}/users
//
// Creates a user that belongs ONLY to this application.
//
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


// ------------------------------------------------------------
// Request
// ------------------------------------------------------------

public sealed record ToggleAuthProviderRequest(
    bool IsEnabled);


public sealed record CreateApplicationUserRequest(
    string Email,
    string Password,
    string Username,
    string DisplayName);