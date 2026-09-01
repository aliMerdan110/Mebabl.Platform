using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.SdkAuth.Refresh;

public sealed class SdkRefreshTokenCommandHandler
    : IRequestHandler<SdkRefreshTokenCommand, SdkRefreshTokenResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenGenerator _jwt;

    public SdkRefreshTokenCommandHandler(
        IApplicationDbContext db,
        IJwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<SdkRefreshTokenResponse> Handle(
    SdkRefreshTokenCommand request,
    CancellationToken cancellationToken)
{
    var token = await _db.RefreshTokens
        .Include(x => x.ApplicationUser)
        .ThenInclude(x => x.Account)
        .FirstOrDefaultAsync(
            x => x.Token == request.RefreshToken,
            cancellationToken);

    if (token is null)
        throw new UnauthorizedAccessException(
            "Invalid refresh token.");

    if (token.IsRevoked)
        throw new UnauthorizedAccessException(
            "Refresh token has been revoked.");

    if (token.IsExpired)
        throw new UnauthorizedAccessException(
            "Refresh token has expired.");

    var roles = await _db.ApplicationUserRoles
        .Where(x =>
            x.ApplicationUserId ==
            token.ApplicationUserId)
        .Select(x => x.Role.Name)
        .Distinct()
        .ToListAsync(cancellationToken);

    var permissions = await _db.ApplicationUserRoles
        .Where(x =>
            x.ApplicationUserId ==
            token.ApplicationUserId)
        .SelectMany(x => x.Role.RolePermissions)
        .Select(x => x.Permission.Code)
        .Distinct()
        .ToListAsync(cancellationToken);

    var accessToken = _jwt.GenerateAccessToken(
        token.ApplicationUser.AccountId,
        token.ApplicationUserId,
        token.ApplicationUser.ApplicationId,
        roles,
        permissions);

    // Revoke old refresh token
    token.RevokedAt = DateTime.UtcNow;

    // Create new refresh token
    var newRefreshToken = new RefreshToken
    {
        ApplicationUserId = token.ApplicationUserId,
        Token = _jwt.GenerateRefreshToken(),
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    };

    _db.RefreshTokens.Add(newRefreshToken);

    await _db.SaveChangesAsync(cancellationToken);

    return new SdkRefreshTokenResponse(
    token.ApplicationUser.AccountId,
    token.ApplicationUserId,
    accessToken,
    newRefreshToken.Token);
}
}