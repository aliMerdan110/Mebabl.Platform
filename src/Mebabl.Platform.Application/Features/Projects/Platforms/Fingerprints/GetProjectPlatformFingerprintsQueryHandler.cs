using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.Fingerprints;

public sealed class GetProjectPlatformFingerprintsQueryHandler
    : IRequestHandler<
        GetProjectPlatformFingerprintsQuery,
        IReadOnlyList<ProjectPlatformFingerprintDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetProjectPlatformFingerprintsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<IReadOnlyList<ProjectPlatformFingerprintDto>> Handle(
        GetProjectPlatformFingerprintsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var platform = await _dbContext.ProjectPlatforms
            .AsNoTracking()
            .Where(x =>
                x.Id == request.PlatformId &&
                x.ProjectId == request.ProjectId &&
                x.Project.DeveloperId == _currentDeveloper.DeveloperId &&
                x.Project.IsActive &&
                !x.Project.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.Platform
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (platform is null)
        {
            throw new KeyNotFoundException(
                "Project platform was not found.");
        }

        if (platform.Platform != "android")
        {
            throw new InvalidOperationException(
                "Fingerprints are only supported for Android platforms.");
        }

        return await _dbContext.ProjectPlatformFingerprints
            .AsNoTracking()
            .Where(x => x.ProjectPlatformId == platform.Id)
            .OrderBy(x => x.FingerprintType)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new ProjectPlatformFingerprintDto(
                x.Id,
                x.ProjectPlatformId,
                x.FingerprintType,
                x.Value,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}