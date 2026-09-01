using Microsoft.AspNetCore.Authorization;

namespace Mebabl.Platform.Infrastructure.Authentication.Authorization;

public sealed class PermissionRequirement
    : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}