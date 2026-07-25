using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Mebabl.Platform.Application.Services.CurrentUser;

namespace Mebabl.Platform.Infrastructure.Services.CurrentUser;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        GetGuidClaim("userId");

    public Guid? AccountId =>
        GetGuidClaim("accountId");

    public Guid? TenantId =>
        GetGuidClaim("tenantId");

    public Guid? ApplicationId =>
        GetGuidClaim("applicationId");

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    private Guid? GetGuidClaim(string claimType)
    {
        var value = User?.FindFirst(claimType)?.Value;

        return Guid.TryParse(value, out var id)
            ? id
            : null;
    }
}