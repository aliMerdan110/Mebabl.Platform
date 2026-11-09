using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.SdkAuth.Refresh;

public sealed class SdkRefreshTokenCommandHandler
    : IRequestHandler<SdkRefreshTokenCommand, SdkRefreshTokenResponse>
{
    private static readonly TimeSpan RefreshTokenLifetime =
        TimeSpan.FromDays(7);

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
        ArgumentNullException.ThrowIfNull(request);

        var suppliedToken = request.RefreshToken;
        if (string.IsNullOrWhiteSpace(suppliedToken) || suppliedToken.Length > 512)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var now = DateTime.UtcNow;
        var suppliedHash = RefreshTokenHashing.Hash(suppliedToken);

        await using var transaction =
            await _db.BeginTransactionAsync(cancellationToken);

        // Supports both legacy plaintext tokens and hashed tokens during migration.
        var current = await _db.RefreshTokens
            .AsNoTracking()
            .Where(x => x.TokenHash == suppliedHash || x.Token == suppliedToken)
            .Select(x => new
            {
                x.Id,
                x.ApplicationUserId,
                x.ExpiresAt,
                x.RevokedAt,
                AccountId = x.ApplicationUser.AccountId,
                ApplicationId = x.ApplicationUser.ApplicationId,
                ProjectId = x.ApplicationUser.Application.ProjectId,
                SecurityStamp = x.ApplicationUser.SecurityStamp,
                UserIsActive = x.ApplicationUser.IsActive,
                UserIsDeleted = x.ApplicationUser.IsDeleted,
                AccountIsDeleted = x.ApplicationUser.Account.IsDeleted,
                ApplicationIsActive = x.ApplicationUser.Application.IsActive,
                ApplicationIsDeleted = x.ApplicationUser.Application.IsDeleted,
                ProjectIsActive = x.ApplicationUser.Application.Project.IsActive,
                ProjectIsDeleted = x.ApplicationUser.Application.Project.IsDeleted,
                x.ApplicationUser.LockoutEnabled,
                x.ApplicationUser.LockoutEnd
            })
            .FirstOrDefaultAsync(cancellationToken);

        var userIsUsable =
            current is not null &&
            current.UserIsActive &&
            !current.UserIsDeleted &&
            !current.AccountIsDeleted &&
            current.ApplicationIsActive &&
            !current.ApplicationIsDeleted &&
            current.ProjectIsActive &&
            !current.ProjectIsDeleted &&
            current.ProjectId != Guid.Empty &&
            (!current.LockoutEnabled ||
             !current.LockoutEnd.HasValue ||
             current.LockoutEnd.Value <= now);

        if (current is null ||
            current.RevokedAt.HasValue ||
            current.ExpiresAt <= now ||
            !userIsUsable)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new UnauthorizedAccessException(
                "Invalid, expired, or revoked refresh token.");
        }

        if (string.IsNullOrWhiteSpace(current.SecurityStamp))
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new UnauthorizedAccessException(
                "The user security state is invalid. Please sign in again.");
        }

        var roles = await _db.ApplicationUserRoles
            .AsNoTracking()
            .Where(x =>
                x.ApplicationUserId == current.ApplicationUserId &&
                x.Role.ApplicationId == current.ApplicationId &&
                x.Role.IsActive &&
                !x.Role.IsDeleted)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions = await _db.ApplicationUserRoles
            .AsNoTracking()
            .Where(x =>
                x.ApplicationUserId == current.ApplicationUserId &&
                x.Role.ApplicationId == current.ApplicationId &&
                x.Role.IsActive &&
                !x.Role.IsDeleted)
            .SelectMany(x => x.Role.RolePermissions)
            .Where(x =>
                x.Permission.ApplicationId == current.ApplicationId &&
                x.Permission.IsActive &&
                !x.Permission.IsDeleted)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var accessToken = _jwt.GenerateAccessToken(
            current.AccountId,
            current.ApplicationUserId,
            current.ApplicationId,
            current.ProjectId,
            current.SecurityStamp,
            roles,
            permissions);

        var newRawToken = _jwt.GenerateRefreshToken();
        var newTokenHash = RefreshTokenHashing.Hash(newRawToken);

        // Atomic conditional revocation prevents concurrent rotation and also
        // verifies that the user still belongs to the same active project.
        var affectedRows = await _db.RefreshTokens
            .Where(x =>
                x.Id == current.Id &&
                x.RevokedAt == null &&
                x.ExpiresAt > now &&
                x.ApplicationUser.SecurityStamp == current.SecurityStamp &&
                x.ApplicationUser.ApplicationId == current.ApplicationId &&
                x.ApplicationUser.Application.ProjectId == current.ProjectId &&
                x.ApplicationUser.IsActive &&
                !x.ApplicationUser.IsDeleted &&
                !x.ApplicationUser.Account.IsDeleted &&
                x.ApplicationUser.Application.IsActive &&
                !x.ApplicationUser.Application.IsDeleted &&
                x.ApplicationUser.Application.Project.IsActive &&
                !x.ApplicationUser.Application.Project.IsDeleted &&
                (!x.ApplicationUser.LockoutEnabled ||
                 !x.ApplicationUser.LockoutEnd.HasValue ||
                 x.ApplicationUser.LockoutEnd <= now))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.RevokedAt, (DateTime?)now)
                    .SetProperty(x => x.Token, (string?)null)
                    .SetProperty(x => x.TokenHash, suppliedHash),
                cancellationToken);

        if (affectedRows != 1)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new UnauthorizedAccessException(
                "Invalid, expired, or revoked refresh token.");
        }

        _db.RefreshTokens.Add(
            new RefreshToken
            {
                ApplicationUserId = current.ApplicationUserId,
                Token = null,
                TokenHash = newTokenHash,
                ExpiresAt = now.Add(RefreshTokenLifetime)
            });

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new SdkRefreshTokenResponse(
            current.AccountId,
            current.ApplicationUserId,
            accessToken,
            newRawToken);
    }
}
