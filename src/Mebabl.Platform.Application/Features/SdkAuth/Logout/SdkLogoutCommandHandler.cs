using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;

namespace Mebabl.Platform.Application.Features.SdkAuth.Logout;

public sealed class SdkLogoutCommandHandler
    : IRequestHandler<SdkLogoutCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public SdkLogoutCommandHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(
        SdkLogoutCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId == Guid.Empty ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated user is required.");
        }

        var rawToken = request.RefreshToken;

        if (string.IsNullOrWhiteSpace(rawToken) ||
            rawToken.Length > 512)
        {
            return;
        }

        var tokenHash = RefreshTokenHashing.Hash(rawToken);
        var userId = _currentUser.UserId;

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationUserId == userId &&
                    (x.TokenHash == tokenHash ||
                     x.Token == rawToken),
                cancellationToken);

        if (token is null)
            return;

        token.RevokedAt ??= DateTime.UtcNow;
        token.TokenHash ??= tokenHash;
        token.Token = null;

        await _db.SaveChangesAsync(cancellationToken);
    }
}