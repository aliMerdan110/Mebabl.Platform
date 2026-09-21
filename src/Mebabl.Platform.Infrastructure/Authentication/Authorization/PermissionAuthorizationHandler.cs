using Microsoft.AspNetCore.Authorization;

namespace Mebabl.Platform.Infrastructure.Authentication.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // يتحقق من وجود الصلاحية داخل هوية المستخدم الموثقة.
        if (context.User.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        if (!context.User.HasClaim("type", "user"))
            return Task.CompletedTask;

        var hasPermission = context.User
            .FindAll("permission")
            .Any(claim =>
                string.Equals(
                    claim.Value,
                    requirement.Permission,
                    StringComparison.Ordinal));

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}