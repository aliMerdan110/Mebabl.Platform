
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Password;

namespace Mebabl.Platform.Application.Features.SdkAuth.ChangePassword;

public sealed class SdkChangePasswordCommandHandler
    : IRequestHandler<SdkChangePasswordCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public SdkChangePasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(
        SdkChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        if (!_currentUser.IsAuthenticated ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                x =>
                    x.Id == userId &&
                    x.ApplicationId == applicationId &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    !x.Account.IsDeleted &&
                    x.Application.IsActive &&
                    !x.Application.IsDeleted,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "The user account is inactive or unavailable.");
        }

        if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !_passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");
        }

        var now = DateTime.UtcNow;

        user.PasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        // Invalidate existing access tokens when SecurityStamp
        // validation is enforced by the JWT authentication pipeline.
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = now;

        // Load active tokens and any legacy tokens that still
        // contain their original plaintext value.
        var refreshTokens = await _dbContext.RefreshTokens
            .Where(x =>
                x.ApplicationUserId == user.Id &&
                (x.RevokedAt == null || x.Token != null))
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            // Preserve a hash before removing a legacy plaintext token.
            if (refreshToken.TokenHash is null &&
                !string.IsNullOrWhiteSpace(refreshToken.Token))
            {
                refreshToken.TokenHash =
                    RefreshTokenHashing.Hash(refreshToken.Token);
            }

            // Never retain the raw refresh token in the database.
            refreshToken.Token = null;

            // Preserve the original revocation time if already revoked.
            refreshToken.RevokedAt ??= now;
        }

        // Save the password change and token revocations together.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
