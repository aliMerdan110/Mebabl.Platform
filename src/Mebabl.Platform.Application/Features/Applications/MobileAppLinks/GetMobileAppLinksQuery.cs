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

public sealed record GetMobileAppLinksQuery
    : IRequest<MobileAppLinkDto?>;

public sealed class GetMobileAppLinksQueryHandler
    : IRequestHandler<GetMobileAppLinksQuery, MobileAppLinkDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public GetMobileAppLinksQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<MobileAppLinkDto?> Handle(
        GetMobileAppLinksQuery request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        return await _db.ApplicationMobileAppLinks
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .Select(x => new MobileAppLinkDto(
                x.Id,
                x.AndroidPackageName,
                x.AndroidSha256CertificateFingerprint,
                x.IosBundleId,
                x.IosTeamId,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }
}