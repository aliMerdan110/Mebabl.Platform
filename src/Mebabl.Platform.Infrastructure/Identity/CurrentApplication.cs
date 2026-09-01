using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Identity;

public sealed class CurrentApplication : ICurrentApplication
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentApplication(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid ApplicationId
    {
        get
        {
            var value = GetClaim("applicationId");

            if (!Guid.TryParse(value, out var applicationId) ||
                applicationId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "The current application is not authenticated.");
            }

            return applicationId;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.Identity?.IsAuthenticated == true &&
                   user.HasClaim("type", "application");
        }
    }

    public Task<bool> ValidateAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(IsAuthenticated);
    }

    private string? GetClaim(string type)
    {
        return _httpContextAccessor.HttpContext?
            .User?
            .FindFirstValue(type);
    }
}