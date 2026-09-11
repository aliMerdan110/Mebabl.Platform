using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route(".well-known")]
public sealed class WellKnownController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public WellKnownController(IApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("assetlinks.json")]
    [Produces("application/json")]
    public async Task<IActionResult> AssetLinks(
        CancellationToken cancellationToken)
    {
        var links = await _db.ApplicationMobileAppLinks
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.AndroidPackageName != null &&
                x.AndroidSha256CertificateFingerprint != null)
            .Select(x => new
            {
                relation = new[]
                {
                    "delegate_permission/common.handle_all_urls"
                },
                target = new
                {
                    @namespace = "android_app",
                    package_name = x.AndroidPackageName!,
                    sha256_cert_fingerprints = new[]
                    {
                        x.AndroidSha256CertificateFingerprint!
                    }
                }
            })
            .ToListAsync(cancellationToken);

        return Ok(links);
    }
}