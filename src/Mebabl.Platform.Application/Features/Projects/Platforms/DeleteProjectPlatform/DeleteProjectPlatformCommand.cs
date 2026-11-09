
using MediatR;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.DeleteProjectPlatform;

public sealed record DeleteProjectPlatformCommand(
    Guid ProjectId,
    Guid PlatformId
) : IRequest;
