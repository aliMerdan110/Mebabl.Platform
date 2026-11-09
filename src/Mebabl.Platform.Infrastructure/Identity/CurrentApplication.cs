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

    private ClaimsPrincipal? CurrentUser =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated
    {
        get
        {
            var user = CurrentUser;

            if (user?.Identity?.IsAuthenticated != true)
                return false;

            // Full application token
            var isSecretApplication =
                user.HasClaim("type", "application") &&
                user.HasClaim("authLevel", "secret");

            // Public application token (SDK registration/login)
            var isPublicApplication =
                user.HasClaim("type", "application_public") &&
                user.HasClaim("scope", "public") &&
                user.HasClaim("authLevel", "public");

            return isSecretApplication || isPublicApplication;
        }
    }

    public Guid ApplicationId
    {
        get
        {
            var user = CurrentUser;

            if (!IsAuthenticated || user is null)
            {
                throw new UnauthorizedAccessException(
                    "The current application is not authenticated.");
            }

            var value = user.FindFirstValue("applicationId");

            if (!Guid.TryParse(value, out var applicationId) ||
                applicationId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "The current application is not authenticated.");
            }

            return applicationId;
        }
    }
}