using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Application.Services.PasswordReset;

namespace Mebabl.Platform.Application.Features.SdkAuth.ResetPassword;

public sealed class SdkResetPasswordCommandHandler
    : IRequestHandler<SdkResetPasswordCommand, SdkResetPasswordResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordResetTokenService _tokenService;

    public SdkResetPasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IPasswordHasher passwordHasher,
        IPasswordResetTokenService tokenService)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<SdkResetPasswordResponse> Handle(
        SdkResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // ------------------------------------------------------------
        // Current Application
        // ------------------------------------------------------------

        var applicationId = _currentApplication.ApplicationId;

        // ------------------------------------------------------------
        // Hash Reset Token
        // ------------------------------------------------------------

        var tokenHash = _tokenService.HashToken(request.Token);

        // ------------------------------------------------------------
        // Find Valid Reset Token
        // ------------------------------------------------------------

        var resetToken =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Include(x => x.User)
                .ThenInclude(x => x.Account)
                .FirstOrDefaultAsync(
                    x =>
                        x.TokenHash == tokenHash &&
                        x.UsedAt == null &&
                        x.ExpiresAt > DateTime.UtcNow &&
                        x.User.ApplicationId == applicationId,
                    cancellationToken);

        if (resetToken is null)
        {
            throw new PasswordResetTokenInvalidException();
        }

        // ------------------------------------------------------------
        // Application User
        // ------------------------------------------------------------

        var user = resetToken.User;

        if (!user.IsActive)
        {
            throw new UserAccountInactiveException();
        }

        if (!user.Account.IsActive)
        {
            throw new UserAccountInactiveException();
        }

        // ------------------------------------------------------------
        // Change Password
        // ------------------------------------------------------------

        user.Account.PasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        user.Account.SecurityStamp =
            Guid.NewGuid().ToString();

        // ------------------------------------------------------------
        // Consume Current Reset Token
        // ------------------------------------------------------------

        var now = DateTime.UtcNow;

        resetToken.UsedAt = now;

        // ------------------------------------------------------------
        // Revoke Other Password Reset Tokens
        // ------------------------------------------------------------

        var otherResetTokens =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Id != resetToken.Id &&
                    x.UsedAt == null)
                .ToListAsync(cancellationToken);

        foreach (var token in otherResetTokens)
        {
            token.UsedAt = now;
        }

        // ------------------------------------------------------------
        // Revoke Existing Refresh Tokens
        // ------------------------------------------------------------

        var refreshTokens =
            await _dbContext.RefreshTokens
                .Where(x =>
                    x.ApplicationUserId == user.Id &&
                    x.RevokedAt == null &&
                    x.ExpiresAt > now)
                .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.RevokedAt = now;
        }

        // ------------------------------------------------------------
        // Save
        // ------------------------------------------------------------

        await _dbContext.SaveChangesAsync(cancellationToken);

        // ------------------------------------------------------------
        // Response
        // ------------------------------------------------------------

        return new SdkResetPasswordResponse(
            "Password has been reset successfully.");
    }
}

// ------------------------------------------------------------
// Exceptions
// ------------------------------------------------------------

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