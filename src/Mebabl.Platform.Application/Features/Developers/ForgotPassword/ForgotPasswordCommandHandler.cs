using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.PasswordReset;
using Mebabl.Platform.Domain.Entities.Identity;
using Microsoft.Extensions.Options;
using Mebabl.Platform.Application.Common.Options;

namespace Mebabl.Platform.Application.Features.Developers.ForgotPassword;

public sealed class ForgotPasswordCommandHandler
    : IRequestHandler<
        ForgotPasswordCommand,
        ForgotPasswordResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ConsoleOptions _consoleOptions;

    public ForgotPasswordCommandHandler(
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

    public async Task<ForgotPasswordResponse> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail =
            request.Email.Trim().ToUpperInvariant();

        var developer =
            await _dbContext.Developers
                .FirstOrDefaultAsync(
                    x =>
                        x.NormalizedEmail == normalizedEmail &&
                        x.IsActive,
                    cancellationToken);

        /*
         * لا نكشف للمستخدم ما إذا كان البريد
         * موجودًا أم لا.
         */
        if (developer is null)
        {
            return new ForgotPasswordResponse(
                "If an account exists with this email, " +
                "you will receive instructions to reset your password.");
        }

        /*
         * إلغاء جميع Reset Tokens السابقة
         * غير المستخدمة.
         */
        var activeTokens =
            await _dbContext.DeveloperPasswordResetTokens
                .Where(x =>
                    x.DeveloperId == developer.Id &&
                    x.UsedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.UsedAt = DateTime.UtcNow;
        }

        /*
         * إنشاء Reset Token جديد.
         */
        var rawToken =
            _tokenService.GenerateToken();

        var tokenHash =
            _tokenService.HashToken(rawToken);

        var resetToken =
            new DeveloperPasswordResetToken
            {
                DeveloperId = developer.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

        _dbContext.DeveloperPasswordResetTokens
            .Add(resetToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

            var consoleUrl =
    _consoleOptions.BaseUrl.TrimEnd('/');

var resetUrl =
    $"{consoleUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

await _emailService.SendAsync(
    developer.Email,
    "Reset your Mebabl password",
    $"""
    Hello {developer.DisplayName},

    We received a request to reset your Mebabl developer password.

    Reset your password using the following link:

    {resetUrl}

    This link will expire in 30 minutes.

    If you did not request this, you can safely ignore this email.

    Mebabl Platform
    """,
    cancellationToken);

        /*
         * لاحقًا:
         * إرسال rawToken عبر Email Service.
         *
         * لا يتم إرجاع الـ Token إلى العميل.
         */
       return new ForgotPasswordResponse(
    "If an account exists with this email, " +
    "you will receive instructions to reset your password.");
    }
}