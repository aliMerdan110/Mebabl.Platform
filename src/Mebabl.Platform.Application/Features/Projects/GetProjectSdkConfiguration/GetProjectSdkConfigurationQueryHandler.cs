using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Projects.DTOs;

namespace Mebabl.Platform.Application.Features.Projects.GetProjectSdkConfiguration;

public sealed class GetProjectSdkConfigurationQueryHandler
    : IRequestHandler<
        GetProjectSdkConfigurationQuery,
        ProjectSdkConfigurationDto>
{
    private const string ApiBaseUrl = "https://api.mebabl.com";

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetProjectSdkConfigurationQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectSdkConfigurationDto> Handle(
        GetProjectSdkConfigurationQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var developerId = _currentDeveloper.DeveloperId;

        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(x =>
                x.Id == request.ProjectId &&
                x.DeveloperId == developerId &&
                x.IsActive &&
                !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                SdkConfiguration = x.SdkConfiguration == null
                    ? null
                    : new
                    {
                        x.SdkConfiguration.ProjectNumber,
                        x.SdkConfiguration.PublicKey,
                        x.SdkConfiguration.ConfigurationVersion,
                        x.SdkConfiguration.IsActive
                    }
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        var sdkConfiguration = project.SdkConfiguration;

        if (sdkConfiguration is null || !sdkConfiguration.IsActive)
        {
            throw new InvalidOperationException(
                "Active project SDK configuration was not found.");
        }

        if (string.IsNullOrWhiteSpace(sdkConfiguration.ProjectNumber) ||
            string.IsNullOrWhiteSpace(sdkConfiguration.PublicKey) ||
            string.IsNullOrWhiteSpace(sdkConfiguration.ConfigurationVersion))
        {
            throw new InvalidOperationException(
                "The project SDK configuration is incomplete.");
        }

        var clients = await (
            from platform in _dbContext.ProjectPlatforms.AsNoTracking()
            where platform.ProjectId == project.Id && platform.IsActive

            join application in _dbContext.Applications.AsNoTracking()
                on new
                {
                    platform.ProjectId,
                    ProjectPlatformId = platform.Id
                }
                equals new
                {
                    application.ProjectId,
                    application.ProjectPlatformId
                }

            where application.IsActive && !application.IsDeleted

            select new
            {
                ApplicationId = application.Id,
                ApplicationName = application.Name,
                ApplicationCode = application.Code,

                PlatformId = platform.Id,
                platform.Platform,
                platform.Nickname,
                platform.PackageName,
                platform.BundleId,
                platform.Domain,

                ApiKey = _dbContext.ApplicationCredentials
                    .Where(credential =>
                        credential.ApplicationId == application.Id &&
                        credential.IsActive)
                    .OrderByDescending(credential => credential.CreatedAt)
                    .Select(credential => credential.ApiKey)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var clientConfigurations = clients
            .Where(client =>
                !string.IsNullOrWhiteSpace(client.ApiKey))
            .Select(client => new ProjectClientDto
            {
                ApplicationName = client.ApplicationName,
                ApplicationCode = client.ApplicationCode,

                ClientInfo = new ProjectClientInfoDto
                {
                    ApplicationId = client.ApplicationId,
                    PlatformId = client.PlatformId,
                    Platform = client.Platform,
                    Nickname = client.Nickname,
                    PackageName = client.PackageName,
                    BundleId = client.BundleId,
                    Domain = client.Domain
                },

                ApiKey = client.ApiKey!
            })
            .ToList();

        return new ProjectSdkConfigurationDto
        {
            ProjectInfo = new ProjectInfoDto
            {
                ProjectId = project.Id,
                ProjectCode = project.Code,
                ProjectName = project.Name,
                BaseUrl = ApiBaseUrl
            },

            ProjectNumber = sdkConfiguration.ProjectNumber,
            PublicKey = sdkConfiguration.PublicKey,
            ConfigurationVersion =
                sdkConfiguration.ConfigurationVersion,

            Client = clientConfigurations
        };
    }
}