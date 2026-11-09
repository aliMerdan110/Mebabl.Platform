
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatforms;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.UpdateProjectPlatform;

public sealed class UpdateProjectPlatformCommandHandler
    : IRequestHandler<UpdateProjectPlatformCommand, ProjectPlatformDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public UpdateProjectPlatformCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectPlatformDto> Handle(
        UpdateProjectPlatformCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var platform = await _dbContext.ProjectPlatforms
            .FirstOrDefaultAsync(
                x => x.Id == request.PlatformId
                    && x.ProjectId == request.ProjectId
                    && x.Project.DeveloperId == _currentDeveloper.DeveloperId
                    && !x.Project.IsDeleted,
                cancellationToken);

        if (platform is null)
        {
            throw new KeyNotFoundException(
                "Project platform not found.");
        }

        var normalizedPlatform = request.Platform.Trim().ToLowerInvariant();

        if (normalizedPlatform is not ("android" or "ios" or "web"))
        {
            throw new ArgumentException(
                "Platform must be android, ios, or web.",
                nameof(request.Platform));
        }

        var duplicate = await _dbContext.ProjectPlatforms
            .AnyAsync(
                x => x.Id != platform.Id
                    && x.ProjectId == request.ProjectId
                    && x.Platform == normalizedPlatform,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "This platform already exists for the project.");
        }

        platform.Platform = normalizedPlatform;
        platform.Nickname = Normalize(request.Nickname);
        platform.PackageName = Normalize(request.PackageName);
        platform.BundleId = Normalize(request.BundleId);
        platform.Domain = Normalize(request.Domain);
        platform.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectPlatformDto(
            platform.Id,
            platform.ProjectId,
            platform.Platform,
            platform.Nickname,
            platform.PackageName,
            platform.BundleId,
            platform.Domain,
            platform.IsActive);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
