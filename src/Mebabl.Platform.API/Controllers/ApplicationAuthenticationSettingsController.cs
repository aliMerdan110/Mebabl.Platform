using MediatR;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.Settings;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/applications/{applicationId:guid}/authentication/settings")]
public class ApplicationAuthenticationSettingsController
    : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetSettings(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetAuthenticationSettingsQuery(
                applicationId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings(
        Guid applicationId,
        UpdateAuthenticationSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new UpdateAuthenticationSettingsCommand(
                applicationId,
                request.AllowRegistration,
                request.RequireEmailVerification,
                request.AllowPasswordAuthentication,
                request.AllowAnonymousAuthentication,
                request.PasswordMinLength,
                request.SessionLifetimeDays,
                request.RefreshTokenLifetimeDays,
                request.MaxLoginAttempts),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record UpdateAuthenticationSettingsRequest(
    bool AllowRegistration,
    bool RequireEmailVerification,
    bool AllowPasswordAuthentication,
    bool AllowAnonymousAuthentication,
    int PasswordMinLength,
    int SessionLifetimeDays,
    int RefreshTokenLifetimeDays,
    int MaxLoginAttempts);