using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.PasswordReset;
using Mebabl.Platform.Domain.Entities.Identity;
using Microsoft.Extensions.Options;
using Mebabl.Platform.Application.Common.Options;

namespace Mebabl.Platform.Application.Features.SdkAuth.ForgotPassword;

public sealed class SdkForgotPasswordCommandHandler
    : IRequestHandler<SdkForgotPasswordCommand, SdkForgotPasswordResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ConsoleOptions _consoleOptions;

    public SdkForgotPasswordCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordResetTokenService tokenService,
        IEmailService emailService,
        IOptions<ConsoleOptions> consoleOptions)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _emailService = emailService;
        _consoleOptions = consoleOptions.Value;
    }

    public async Task<SdkForgotPasswordResponse> Handle(
        SdkForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        // البحث في جدول الـ Accounts المرتبط بمستخدمي الـ SDK
        var user = await _dbContext.ApplicationUsers
            .Include(x => x.Account) // جلب الـ Account المرتبط
            .FirstOrDefaultAsync(
                x => x.Account.NormalizedEmail == normalizedEmail && x.IsActive,
                cancellationToken);

        if (user is null)
        {
            return new SdkForgotPasswordResponse(
                "If an account exists with this email, you will receive instructions to reset your password.");
        }

        var activeTokens = await _dbContext.ApplicationUserPasswordResetTokens
            .Where(x => x.UserId == user.Id && x.UsedAt == null && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.UsedAt = DateTime.UtcNow;
        }

        var rawToken = _tokenService.GenerateToken();
        var tokenHash = _tokenService.HashToken(rawToken);

        var resetToken = new ApplicationUserPasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _dbContext.ApplicationUserPasswordResetTokens.Add(resetToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var consoleUrl = _consoleOptions.BaseUrl.TrimEnd('/');
        var resetUrl = $"{consoleUrl}/sdk/reset-password?token={Uri.EscapeDataString(rawToken)}";

        await _emailService.SendAsync(
            user.Account.Email,
            "Reset your password",
            $"""
            Hello,

            We received a request to reset your password.

            Reset your password using the following link:

            {resetUrl}

            This link will expire in 30 minutes.

            If you did not request this, you can safely ignore this email.

            Mebabl Platform
            """,
            cancellationToken);

        return new SdkForgotPasswordResponse(
            "If an account exists with this email, you will receive instructions to reset your password.");
    }
}