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

        var resetToken =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Include(x => x.User)
                .ThenInclude(u => u.Account) // جلب الحساب المرتبط
                .FirstOrDefaultAsync(
                    x =>
                        x.TokenHash == tokenHash &&
                        x.UsedAt == null &&
                        x.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

        if (resetToken is null)
        {
            throw new PasswordResetTokenInvalidException();
        }

        var user = resetToken.User;

        if (!user.IsActive)
        {
            throw new UserAccountInactiveException();
        }

        // تغيير كلمة المرور في جدول الـ Account
        user.Account.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.Account.SecurityStamp = Guid.NewGuid().ToString(); // تحديث الطابع الأمني إن أمكن

        // استهلاك التوكن الحالي
        resetToken.UsedAt = DateTime.UtcNow;

        // إلغاء بقية توكنات إعادة التعيين
        var otherResetTokens =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Where(x =>
                    x.UserId == user.Id &&
                    x.Id != resetToken.Id &&
                    x.UsedAt == null)
                .ToListAsync(cancellationToken);

        foreach (var token in otherResetTokens)
        {
            token.UsedAt = DateTime.UtcNow;
        }

        // إلغاء الـ Refresh Tokens (تأكد هل الخاصية اسمها AccountId أو UserId في RefreshTokens)
        // إلغاء الـ Refresh Tokens النشطة الخاصة بمستخدم الـ SDK
        var refreshTokens =
            await _dbContext.RefreshTokens
                .Where(x =>
                    x.ApplicationUserId == user.Id &&
                    x.RevokedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SdkResetPasswordResponse("Password has been reset successfully.");
    }
}

// الاستثناءات الخاصة بالمستخدمين
public sealed class PasswordResetTokenInvalidException : Exception
{
    public PasswordResetTokenInvalidException()
        : base("The password reset token is invalid or has expired.") { }
}

public sealed class UserAccountInactiveException : Exception
{
    public UserAccountInactiveException()
        : base("The user account is inactive.") { }
}