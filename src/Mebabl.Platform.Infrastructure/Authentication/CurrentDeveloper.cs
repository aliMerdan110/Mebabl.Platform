using System.Security.Claims;

using Microsoft.AspNetCore.Http;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Authentication;

public sealed class CurrentDeveloper : ICurrentDeveloper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentDeveloper(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // يقرأ هوية المطور الموثقة من Claims فقط.
    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true &&
        Principal.HasClaim("type", "developer") &&
        TryGetDeveloperId(out _);

    public Guid DeveloperId
    {
        get
        {
            if (!IsAuthenticated ||
                !TryGetDeveloperId(out var developerId))
            {
                throw new UnauthorizedAccessException(
                    "The current developer is not authenticated.");
            }

            return developerId;
        }
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    private bool TryGetDeveloperId(
        out Guid developerId)
    {
        developerId = Guid.Empty;

        var value = Principal?
            .FindFirstValue("developerId");

        return Guid.TryParse(value, out developerId) &&
               developerId != Guid.Empty;
    }
}