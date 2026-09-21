using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Services.CurrentUser;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true &&
        Principal.HasClaim("type", "user");

    public Guid UserId =>
        GetRequiredGuidClaim("userId");

    public Guid AccountId =>
        GetRequiredGuidClaim("accountId");

    public Guid ApplicationId =>
        GetRequiredGuidClaim("applicationId");

    private Guid GetRequiredGuidClaim(
        string claimType)
    {
        if (!IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "The current user is not authenticated.");
        }

        var value = Principal?
            .FindFirstValue(claimType);

        if (!Guid.TryParse(value, out var id) ||
            id == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                $"The current user does not contain a valid '{claimType}' claim.");
        }

        return id;
    }
}