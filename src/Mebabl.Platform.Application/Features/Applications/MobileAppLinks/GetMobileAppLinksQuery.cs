using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

public sealed record MobileAppLinkDto(
    Guid Id,
    string? AndroidPackageName,
    string? AndroidSha256CertificateFingerprint,
    string? IosBundleId,
    string? IosTeamId,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record GetMobileAppLinksQuery(
    Guid ApplicationId
) : IRequest<MobileAppLinkDto?>;

public sealed class GetMobileAppLinksQueryHandler
    : IRequestHandler<GetMobileAppLinksQuery, MobileAppLinkDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetMobileAppLinksQueryHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<MobileAppLinkDto?> Handle(
        GetMobileAppLinksQuery request,
        CancellationToken cancellationToken)
    {
        var applicationExists = await _db.Applications
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (!applicationExists)
            throw new KeyNotFoundException(
                "Application not found.");

        return await _db.ApplicationMobileAppLinks
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == request.ApplicationId)
            .Select(x => new MobileAppLinkDto(
                x.Id,
                x.AndroidPackageName,
                x.AndroidSha256CertificateFingerprint,
                x.IosBundleId,
                x.IosTeamId,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}