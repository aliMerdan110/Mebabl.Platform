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

    public Guid DeveloperId
    {
        get
        {
            var claim = _httpContextAccessor
                .HttpContext?
                .User?
                .FindFirst("developerId");

            return claim is null
                ? Guid.Empty
                : Guid.Parse(claim.Value);
        }
    }

    public bool IsAuthenticated =>
        DeveloperId != Guid.Empty;
}