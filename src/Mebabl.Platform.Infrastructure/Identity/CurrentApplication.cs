using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;


namespace Mebabl.Platform.Infrastructure.Identity;

public sealed class CurrentApplication : ICurrentApplication
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _dbContext;

    private Guid? _applicationId;
    private bool? _isAuthenticated;

    public CurrentApplication(
        IHttpContextAccessor httpContextAccessor,
        IApplicationDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public Guid ApplicationId
    {
        get
        {
            if (_applicationId is null || _applicationId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "The current application is not authenticated.");
            }

            return _applicationId.Value;
        }
    }

    public bool IsAuthenticated =>
        _isAuthenticated == true;

    public async Task<bool> ValidateAsync(
        CancellationToken cancellationToken = default)
    {
        if (_isAuthenticated.HasValue)
            return _isAuthenticated.Value;

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            _isAuthenticated = false;
            return false;
        }

        var applicationIdHeader =
            httpContext.Request.Headers["X-Application-Id"]
                .FirstOrDefault();

        var apiKey =
            httpContext.Request.Headers["X-Api-Key"]
                .FirstOrDefault();

        if (!Guid.TryParse(
                applicationIdHeader,
                out var applicationId) ||
            applicationId == Guid.Empty ||
            string.IsNullOrWhiteSpace(apiKey))
        {
            _isAuthenticated = false;
            return false;
        }

        var credential = await _dbContext.ApplicationCredentials
            .AsNoTracking()
            .Include(x => x.Application)
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.ApiKey == apiKey &&
                    x.IsActive &&
                    x.Application.IsActive,
                cancellationToken);

        if (credential is null)
        {
            _isAuthenticated = false;
            return false;
        }

        _applicationId = applicationId;
        _isAuthenticated = true;

        return true;
    }
}