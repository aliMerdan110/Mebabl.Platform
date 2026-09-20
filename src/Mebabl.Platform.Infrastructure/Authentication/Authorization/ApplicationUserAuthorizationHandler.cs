using Microsoft.AspNetCore.Authorization;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Authentication.Authorization;

public sealed class ApplicationUserAuthorizationHandler
    : AuthorizationHandler<ApplicationUserRequirement>
{
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public ApplicationUserAuthorizationHandler(
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApplicationUserRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated == true)
            return Task.CompletedTask;

        if (!context.User.HasClaim("type", "user"))
            return Task.CompletedTask;

        if (!_currentUser.IsAuthenticated)
            return Task.CompletedTask;

        if (!_currentApplication.IsAuthenticated)
            return Task.CompletedTask;

        var tokenApplicationId = context.User
            .FindFirst("applicationId")
            ?.Value;

        if (!Guid.TryParse(
                tokenApplicationId,
                out var tokenApplicationGuid))
        {
            return Task.CompletedTask;
        }

        if (tokenApplicationGuid == Guid.Empty)
            return Task.CompletedTask;

        if (tokenApplicationGuid !=
            _currentApplication.ApplicationId)
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}