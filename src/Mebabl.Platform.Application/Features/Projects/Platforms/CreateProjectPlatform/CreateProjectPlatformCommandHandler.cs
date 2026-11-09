using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.CreateProjectPlatform;

public sealed class CreateProjectPlatformCommandHandler
    : IRequestHandler<CreateProjectPlatformCommand, CreateProjectPlatformResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public CreateProjectPlatformCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<CreateProjectPlatformResponse> Handle(
        CreateProjectPlatformCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == request.ProjectId
                     && x.DeveloperId == _currentDeveloper.DeveloperId
                     && x.IsActive
                     && !x.IsDeleted,
                cancellationToken);

        if (project is null)
            throw new KeyNotFoundException("Project not found.");

        var platformName = request.Platform.Trim().ToLowerInvariant();

        if (platformName is not ("android" or "ios" or "web"))
            throw new ArgumentException(
                "Platform must be android, ios, or web.",
                nameof(request.Platform));

        var exists = await _dbContext.ProjectPlatforms
            .AnyAsync(
                x => x.ProjectId == project.Id
                     && x.Platform == platformName,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                "This platform already exists for the project.");

        var platform = new ProjectPlatform
        {
            ProjectId = project.Id,
            Platform = platformName,
            Nickname = Normalize(request.Nickname),
            PackageName = Normalize(request.PackageName),
            BundleId = Normalize(request.BundleId),
            Domain = Normalize(request.Domain),
            IsActive = true
        };

        _dbContext.ProjectPlatforms.Add(platform);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateProjectPlatformResponse(
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