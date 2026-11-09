using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.Fingerprints;

public sealed class DeleteProjectPlatformFingerprintCommandHandler
    : IRequestHandler<DeleteProjectPlatformFingerprintCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public DeleteProjectPlatformFingerprintCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        DeleteProjectPlatformFingerprintCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var fingerprint = await _dbContext.ProjectPlatformFingerprints
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.FingerprintId &&
                    x.ProjectPlatformId == request.PlatformId &&
                    x.ProjectPlatform.ProjectId == request.ProjectId &&
                    x.ProjectPlatform.Platform == "android" &&
                    x.ProjectPlatform.Project.DeveloperId ==
                        _currentDeveloper.DeveloperId &&
                    x.ProjectPlatform.Project.IsActive &&
                    !x.ProjectPlatform.Project.IsDeleted,
                cancellationToken);

        if (fingerprint is null)
        {
            throw new KeyNotFoundException(
                "Android fingerprint was not found.");
        }

        _dbContext.ProjectPlatformFingerprints.Remove(fingerprint);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}