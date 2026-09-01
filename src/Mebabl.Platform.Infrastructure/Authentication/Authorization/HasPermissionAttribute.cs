using Microsoft.AspNetCore.Authorization;

namespace Mebabl.Platform.Infrastructure.Authentication.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = permission;
    }
}