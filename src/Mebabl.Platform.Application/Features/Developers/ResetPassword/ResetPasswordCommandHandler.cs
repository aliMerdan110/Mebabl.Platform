
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Application.Services.PasswordReset;

namespace Mebabl.Platform.Application.Features.Developers.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetTokenService _tokenService;

    public ResetPasswordCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IPasswordResetTokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tokenHash = _tokenService.HashToken(request.Token);

        var resetToken =
            await _dbContext.DeveloperPasswordResetTokens
                .Include(x => x.Developer)
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

        var developer = resetToken.Developer;

        if (!developer.IsActive || developer.IsDeleted)
        {
            throw new DeveloperAccountInactiveException();
        }

        // Update the developer password.
        developer.PasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        developer.UpdatedAt = now;

        // Consume the reset token.
        resetToken.UsedAt = now;

        // Invalidate other outstanding password-reset tokens.
        var otherResetTokens =
            await _dbContext.DeveloperPasswordResetTokens
                .Where(x =>
                    x.DeveloperId == developer.Id &&
                    x.Id != resetToken.Id &&
                    x.UsedAt == null)
                .ToListAsync(cancellationToken);

        foreach (var token in otherResetTokens)
        {
            token.UsedAt = now;
        }

        // Load active refresh tokens and any legacy tokens
        // that still store the original plaintext value.
        var refreshTokens =
            await _dbContext.DeveloperRefreshTokens
                .Where(x =>
                    x.DeveloperId == developer.Id &&
                    (x.RevokedAt == null || x.Token != null))
                .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            // Preserve the hash before removing a legacy plaintext token.
            if (refreshToken.TokenHash is null &&
                !string.IsNullOrWhiteSpace(refreshToken.Token))
            {
                refreshToken.TokenHash =
                    RefreshTokenHashing.Hash(refreshToken.Token);
            }

            refreshToken.Token = null;
            refreshToken.RevokedAt ??= now;
        }

        // Save all password-reset and session-revocation changes together.
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResponse(
            "Password has been reset successfully.");
    }
}

// Invalid, expired, or previously used reset token.
public sealed class PasswordResetTokenInvalidException : Exception
{
    public PasswordResetTokenInvalidException()
        : base("The password reset token is invalid or has expired.")
    {
    }
}

// Inactive or deleted developer account.
public sealed class DeveloperAccountInactiveException : Exception
{
    public DeveloperAccountInactiveException()
        : base("The developer account is inactive.")
    {
    }
}
