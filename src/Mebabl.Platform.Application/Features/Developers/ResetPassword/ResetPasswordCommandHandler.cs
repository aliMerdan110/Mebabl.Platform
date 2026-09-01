
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Application.Services.PasswordReset;

namespace Mebabl.Platform.Application.Features.Developers.ResetPassword;

public sealed class ResetPasswordCommandHandler
    : IRequestHandler<
        ResetPasswordCommand,
        ResetPasswordResponse>
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
        var tokenHash =
            _tokenService.HashToken(request.Token);

        var resetToken =
            await _dbContext.DeveloperPasswordResetTokens
                .Include(x => x.Developer)
                .FirstOrDefaultAsync(
                    x =>
                        x.TokenHash == tokenHash &&
                        x.UsedAt == null &&
                        x.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

        // Token غير صالح أو منتهي أو مستخدم سابقًا
        if (resetToken is null)
        {
            throw new PasswordResetTokenInvalidException();
        }

        var developer = resetToken.Developer;

        // الحساب غير فعال
        if (!developer.IsActive)
        {
            throw new DeveloperAccountInactiveException();
        }

        // تغيير كلمة المرور
        developer.PasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        // استهلاك Reset Token الحالي
        resetToken.UsedAt = DateTime.UtcNow;

        // إلغاء جميع Reset Tokens الأخرى
        var otherResetTokens =
            await _dbContext.DeveloperPasswordResetTokens
                .Where(x =>
                    x.DeveloperId == developer.Id &&
                    x.Id != resetToken.Id &&
                    x.UsedAt == null)
                .ToListAsync(cancellationToken);

        foreach (var token in otherResetTokens)
        {
            token.UsedAt = DateTime.UtcNow;
        }

        // إلغاء جميع Refresh Tokens النشطة
        var refreshTokens =
            await _dbContext.DeveloperRefreshTokens
                .Where(x =>
                    x.DeveloperId == developer.Id &&
                    x.RevokedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new ResetPasswordResponse(
            "Password has been reset successfully.");
    }
}


// Reset Token غير صالح / منتهي / مستخدم سابقًا
public sealed class PasswordResetTokenInvalidException
    : Exception
{
    public PasswordResetTokenInvalidException()
        : base("The password reset token is invalid or has expired.")
    {
    }
}


// حساب Developer غير فعال
public sealed class DeveloperAccountInactiveException
    : Exception
{
    public DeveloperAccountInactiveException()
        : base("The developer account is inactive.")
    {
    }
}
