using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using Mebabl.Platform.Infrastructure.Authentication.Authorization;

namespace Mebabl.Platform.Infrastructure.Authentication;

public sealed class PermissionPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    // ينشئ سياسة صلاحية ديناميكية للمستخدم.
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
            return policy;

        return new AuthorizationPolicyBuilder(
                JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireClaim("type", "user")
            .RequireClaim("userId")
            .RequireClaim("accountId")
            .RequireClaim("applicationId")
            .AddRequirements(
                new PermissionRequirement(policyName))
            .Build();
    }
}