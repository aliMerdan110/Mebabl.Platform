using MediatR;

namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.CreateApplicationPlatform;

public sealed record CreateApplicationPlatformCommand(
    Guid ApplicationId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain
) : IRequest<CreateApplicationPlatformResponse>;

public sealed record CreateApplicationPlatformResponse(
    Guid Id,
    Guid ApplicationId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain,
    bool IsActive
);