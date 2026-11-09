using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Authentication.Authorization;

/// <summary>
/// Ensures the authenticated user claims match the scoped identity resolved
/// for the current request, including project, application, account and user.
/// </summary>
public sealed class ApplicationUserAuthorizationHandler
    : AuthorizationHandler<ApplicationUserRequirement>
{
    private readonly ICurrentUser _currentUser;

    public ApplicationUserAuthorizationHandler(ICurrentUser currentUser)
    {
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApplicationUserRequirement requirement)
    {
        var principal = context.User;

        if (principal.Identity?.IsAuthenticated != true ||
            !principal.HasClaim("type", "user") ||
            !_currentUser.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        if (!TryGetGuidClaim(principal, "projectId", out var tokenProjectId) ||
            !TryGetGuidClaim(principal, "applicationId", out var tokenApplicationId) ||
            !TryGetGuidClaim(principal, "accountId", out var tokenAccountId) ||
            !TryGetGuidClaim(principal, "userId", out var tokenUserId))
        {
            return Task.CompletedTask;
        }

        if (_currentUser.ProjectId != tokenProjectId ||
            _currentUser.ApplicationId != tokenApplicationId ||
            _currentUser.AccountId != tokenAccountId ||
            _currentUser.UserId != tokenUserId)
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }

    private static bool TryGetGuidClaim(
        ClaimsPrincipal principal,
        string claimType,
        out Guid value)
    {
        value = Guid.Empty;

        return Guid.TryParse(
                   principal.FindFirst(claimType)?.Value,
                   out value) &&
               value != Guid.Empty;
    }
}
