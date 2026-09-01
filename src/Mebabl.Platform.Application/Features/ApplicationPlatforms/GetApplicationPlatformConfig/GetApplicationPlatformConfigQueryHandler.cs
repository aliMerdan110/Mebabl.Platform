using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.GetApplicationPlatformConfig;

public sealed class GetApplicationPlatformConfigQueryHandler
    : IRequestHandler<
        GetApplicationPlatformConfigQuery,
        ApplicationPlatformConfigResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public GetApplicationPlatformConfigQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationPlatformConfigResponse> Handle(
        GetApplicationPlatformConfigQuery request,
        CancellationToken cancellationToken)
    {
        // ------------------------------------------------------------
        // Application
        // ------------------------------------------------------------

        var applicationExists = await _dbContext.Applications
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.ApplicationId,
                cancellationToken);

        if (!applicationExists)
        {
            throw new KeyNotFoundException(
                "Application not found.");
        }

        // ------------------------------------------------------------
        // Platform
        // ------------------------------------------------------------

        var platform = await _dbContext.ApplicationPlatforms
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.PlatformId &&
                    x.ApplicationId == request.ApplicationId,
                cancellationToken);

        if (platform is null)
        {
            throw new KeyNotFoundException(
                "Application platform not found.");
        }

        // ------------------------------------------------------------
        // Active Credential
        // ------------------------------------------------------------

        var credential = await _dbContext.ApplicationCredentials
            .AsNoTracking()
            .Where(
                x =>
                    x.ApplicationId == request.ApplicationId &&
                    x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
.FirstOrDefaultAsync(cancellationToken);

        if (credential is null)
        {
            throw new InvalidOperationException(
                "No active application credential was found.");
        }

        // ------------------------------------------------------------
        // Configuration
        // ------------------------------------------------------------

        return new ApplicationPlatformConfigResponse(
            ApplicationId: platform.ApplicationId,
            PlatformId: platform.Id,
            Platform: platform.Platform,
            PackageName: platform.PackageName,
            BundleId: platform.BundleId,
            Domain: platform.Domain,
            ApiKey: credential.ApiKey,
            BaseUrl: "https://api.mebabl.com"
        );
    }
}