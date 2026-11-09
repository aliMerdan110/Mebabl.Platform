
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Application.Services.PasswordReset;

namespace Mebabl.Platform.Application.Features.SdkAuth.ResetPassword;

public sealed class SdkResetPasswordCommandHandler
    : IRequestHandler<SdkResetPasswordCommand, SdkResetPasswordResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetTokenService _tokenService;

    public SdkResetPasswordCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IPasswordResetTokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<SdkResetPasswordResponse> Handle(
        SdkResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tokenHash = _tokenService.HashToken(request.Token);

        var resetToken =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(
                    x =>
                        x.TokenHash == tokenHash &&
                        x.UsedAt == null &&
                        x.ExpiresAt > now,
                    cancellationToken);

        if (resetToken is null)
        {
            throw new PasswordResetTokenInvalidException();
        }

        var user = resetToken.ApplicationUser;

        if (!user.IsActive || user.IsDeleted)
        {
            throw new UserAccountInactiveException();
        }

        user.PasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        // Access tokens become invalid when SecurityStamp validation
        // is enforced by the JWT authentication pipeline.
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = now;

        // Consume the reset token.
        resetToken.UsedAt = now;

        // Invalidate other outstanding password-reset tokens.
        var otherResetTokens =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Where(x =>
                    x.ApplicationUserId == user.Id &&
                    x.Id != resetToken.Id &&
                    x.UsedAt == null)
                .ToListAsync(cancellationToken);

        foreach (var token in otherResetTokens)
        {
            token.UsedAt = now;
        }

        // Load active refresh tokens and legacy plaintext tokens,
        // including expired tokens that still contain plaintext.
        var refreshTokens = await _dbContext.RefreshTokens
            .Where(x =>
                x.ApplicationUserId == user.Id &&
                (x.RevokedAt == null || x.Token != null))
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            // Hash legacy tokens before clearing their plaintext values.
            if (refreshToken.TokenHash is null &&
                !string.IsNullOrWhiteSpace(refreshToken.Token))
            {
                refreshToken.TokenHash =
                    RefreshTokenHashing.Hash(refreshToken.Token);
            }

            refreshToken.Token = null;
            refreshToken.RevokedAt ??= now;
        }

        // Save the password reset, reset-token consumption,
        // and refresh-token revocations together.
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SdkResetPasswordResponse(
            "Password has been reset successfully.");
    }
}

public sealed class PasswordResetTokenInvalidException : Exception
{
    public PasswordResetTokenInvalidException()
        : base("The password reset token is invalid or has expired.")
    {
    }
}

public sealed class UserAccountInactiveException : Exception
{
    public UserAccountInactiveException()
        : base("The user account is inactive.")
    {
    }
}
