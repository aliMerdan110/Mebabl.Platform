using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.GetApplicationPlatforms;

public sealed class GetApplicationPlatformsQueryHandler
    : IRequestHandler<
        GetApplicationPlatformsQuery,
        IReadOnlyList<ApplicationPlatformResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetApplicationPlatformsQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ApplicationPlatformResponse>> Handle(
        GetApplicationPlatformsQuery request,
        CancellationToken cancellationToken)
    {
        var applicationExists = await _dbContext.Applications
            .AnyAsync(
                x => x.Id == request.ApplicationId,
                cancellationToken);

        if (!applicationExists)
        {
            throw new KeyNotFoundException(
                "Application not found.");
        }

        return await _dbContext.ApplicationPlatforms
            .AsNoTracking()
            .Where(x => x.ApplicationId == request.ApplicationId)
            .OrderBy(x => x.Platform)
            .Select(x => new ApplicationPlatformResponse(
                x.Id,
                x.ApplicationId,
                x.Platform,
                x.Nickname,
                x.PackageName,
                x.BundleId,
                x.Domain,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}