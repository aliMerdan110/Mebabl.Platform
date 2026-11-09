using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatformConfig;

public sealed class GetProjectPlatformConfigQueryHandler
    : IRequestHandler<
        GetProjectPlatformConfigQuery,
        ProjectPlatformConfigResponse>
{
    private const string ApiBaseUrl = "https://api.mebabl.com";

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetProjectPlatformConfigQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectPlatformConfigResponse> Handle(
        GetProjectPlatformConfigQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var project = await _dbContext.Projects
            .AsNoTracking()
            .Include(x => x.SdkConfiguration)
            .SingleOrDefaultAsync(
                x => x.Id == request.ProjectId
                    && x.DeveloperId == _currentDeveloper.DeveloperId
                    && !x.IsDeleted,
                cancellationToken);

        if (project is null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        if (!project.IsActive)
        {
            throw new InvalidOperationException(
                "The project is inactive.");
        }

        var sdkConfiguration = project.SdkConfiguration;

        if (sdkConfiguration is null || !sdkConfiguration.IsActive)
        {
            throw new InvalidOperationException(
                "Active SDK configuration has not been created for this project.");
        }

        if (string.IsNullOrWhiteSpace(sdkConfiguration.ProjectNumber)
            || string.IsNullOrWhiteSpace(sdkConfiguration.PublicKey)
            || string.IsNullOrWhiteSpace(sdkConfiguration.ConfigurationVersion))
        {
            throw new InvalidOperationException(
                "The project SDK configuration is incomplete.");
        }

        var platform = await _dbContext.ProjectPlatforms
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == request.PlatformId
                    && x.ProjectId == request.ProjectId,
                cancellationToken);

        if (platform is null)
        {
            throw new KeyNotFoundException(
                "Project platform not found.");
        }

        if (!platform.IsActive)
        {
            throw new InvalidOperationException(
                "The project platform is inactive.");
        }

        var application = await _dbContext.Applications
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == request.ApplicationId
                    && x.ProjectId == request.ProjectId
                    && x.ProjectPlatformId == request.PlatformId
                    && !x.IsDeleted,
                cancellationToken);

        if (application is null)
        {
            throw new KeyNotFoundException(
                "Application not found or is not associated with this platform.");
        }

        if (!application.IsActive)
        {
            throw new InvalidOperationException(
                "The application is inactive.");
        }

        var credential = await _dbContext.ApplicationCredentials
            .AsNoTracking()
            .Where(
                x => x.ApplicationId == application.Id
                    && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (credential is null)
        {
            throw new InvalidOperationException(
                "No active application credential was found.");
        }

        if (string.IsNullOrWhiteSpace(credential.ApiKey))
        {
            throw new InvalidOperationException(
                "The application API key is missing.");
        }

        return new ProjectPlatformConfigResponse(
            ConfigurationVersion: sdkConfiguration.ConfigurationVersion,
            ProjectId: project.Id,
            ProjectNumber: sdkConfiguration.ProjectNumber,
            ApplicationId: application.Id,
            ApplicationName: application.Name,
            ApplicationCode: application.Code,
            PlatformId: platform.Id,
            Platform: platform.Platform,
            Nickname: platform.Nickname,
            PackageName: platform.PackageName,
            BundleId: platform.BundleId,
            Domain: platform.Domain,
            PublicKey: sdkConfiguration.PublicKey,
            ApiKey: credential.ApiKey,
            BaseUrl: ApiBaseUrl);
    }
}