
namespace Mebabl.Platform.Application.Features.Projects.DTOs;

public sealed class ProjectSdkConfigurationDto
{
    public ProjectInfoDto ProjectInfo { get; init; } = default!;

    public IReadOnlyList<ProjectClientDto> Client { get; init; }
        = Array.Empty<ProjectClientDto>();

    public string ConfigurationVersion { get; init; } = "1";
}

public sealed class ProjectInfoDto
{
    public Guid ProjectId { get; init; }

    public string ProjectCode { get; init; } = string.Empty;

    public string ProjectName { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;
}

public sealed class ProjectClientDto
{
    public ProjectClientInfoDto ClientInfo { get; init; } = default!;

    public string ApiKey { get; init; } = string.Empty;
}

public sealed class ProjectClientInfoDto
{
    public Guid ApplicationId { get; init; }

    public Guid PlatformId { get; init; }

    public string Platform { get; init; } = string.Empty;

    public string? PackageName { get; init; }

    public string? BundleId { get; init; }

    public string? Domain { get; init; }
}
