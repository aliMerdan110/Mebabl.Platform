
using MediatR;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.CreateProjectPlatform;

public sealed record CreateProjectPlatformCommand(
    Guid ProjectId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain
) : IRequest<CreateProjectPlatformResponse>;
