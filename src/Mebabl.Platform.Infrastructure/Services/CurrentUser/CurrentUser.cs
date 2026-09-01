using System.Security.Claims;
using Mebabl.Platform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Mebabl.Platform.Infrastructure.Services.CurrentUser;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public Guid UserId =>
        GetGuidClaim("userId");

    public Guid AccountId =>
        GetGuidClaim("accountId");

    public Guid ApplicationId =>
        GetGuidClaim("applicationId");


    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;


    private Guid GetGuidClaim(string claimType)
    {
        var value = User?.FindFirst(claimType)?.Value;

        return Guid.TryParse(value, out var id)
            ? id
            : Guid.Empty;
    }
}