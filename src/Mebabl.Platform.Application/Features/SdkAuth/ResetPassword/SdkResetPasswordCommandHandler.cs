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
        var tokenHash = _tokenService.HashToken(request.Token);
        var now = DateTime.UtcNow;

        var resetToken =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Include(x => x.User)
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

        var user = resetToken.User;

        if (!user.IsActive || user.IsDeleted)
        {
            throw new UserAccountInactiveException();
        }

        user.PasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        user.SecurityStamp =
            Guid.NewGuid().ToString();

        user.UpdatedAt = now;

        resetToken.UsedAt = now;

        var otherResetTokens =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Where(
                    x =>
                        x.UserId == user.Id &&
                        x.Id != resetToken.Id &&
                        x.UsedAt == null)
                .ToListAsync(cancellationToken);

        foreach (var token in otherResetTokens)
        {
            token.UsedAt = now;
        }

        var refreshTokens =
            await _dbContext.RefreshTokens
                .Where(
                    x =>
                        x.ApplicationUserId == user.Id &&
                        x.RevokedAt == null &&
                        x.ExpiresAt > now)
                .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.RevokedAt = now;
        }

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