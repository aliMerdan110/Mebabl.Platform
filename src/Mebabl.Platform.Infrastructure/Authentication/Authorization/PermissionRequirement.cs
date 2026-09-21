using Microsoft.AspNetCore.Authorization;

namespace Mebabl.Platform.Infrastructure.Authentication.Authorization;

public sealed class PermissionRequirement
    : IAuthorizationRequirement
{
    // يمثل صلاحية واحدة مطلوبة للوصول إلى مورد محمي.
    public PermissionRequirement(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            throw new ArgumentException(
                "Permission is required.",
                nameof(permission));
        }

        Permission = permission;
    }

    public string Permission { get; }
}