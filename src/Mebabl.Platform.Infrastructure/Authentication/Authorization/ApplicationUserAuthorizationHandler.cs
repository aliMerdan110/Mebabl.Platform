using Microsoft.AspNetCore.Authorization;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Authentication.Authorization;

public sealed class ApplicationUserAuthorizationHandler
    : AuthorizationHandler<ApplicationUserRequirement>
{
    private readonly ICurrentUser _currentUser;

    public ApplicationUserAuthorizationHandler(
        ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApplicationUserRequirement requirement)
    {
        // يتحقق من تطابق هوية المستخدم والتطبيق مع Claims الحالية.
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        if (!context.User.HasClaim("type", "user"))
            return Task.CompletedTask;

        if (!_currentUser.IsAuthenticated)
            return Task.CompletedTask;

        var applicationIdClaim = context.User
            .FindFirst("applicationId")
            ?.Value;

        if (!Guid.TryParse(
                applicationIdClaim,
                out var tokenApplicationId) ||
            tokenApplicationId == Guid.Empty)
        {
            return Task.CompletedTask;
        }

        var userIdClaim = context.User
            .FindFirst("userId")
            ?.Value;

        if (!Guid.TryParse(
                userIdClaim,
                out var tokenUserId) ||
            tokenUserId == Guid.Empty)
        {
            return Task.CompletedTask;
        }

        if (_currentUser.ApplicationId != tokenApplicationId)
            return Task.CompletedTask;

        if (_currentUser.UserId != tokenUserId)
            return Task.CompletedTask;

        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}