using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkAuth.Logout;

public sealed class SdkLogoutCommandHandler
    : IRequestHandler<SdkLogoutCommand>
{
    private readonly IApplicationDbContext _db;

    public SdkLogoutCommandHandler(
        IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task Handle(
        SdkLogoutCommand request,
        CancellationToken cancellationToken)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.Token == request.RefreshToken,
                cancellationToken);

        if (token is null)
            return;

        // Revoke refresh token instead of deleting it.
        token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}