using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Options;
using Mebabl.Platform.Application.Services.PasswordReset;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Application.Services.Email;

namespace Mebabl.Platform.Application.Features.SdkAuth.ForgotPassword;

public sealed class SdkForgotPasswordCommandHandler
    : IRequestHandler<SdkForgotPasswordCommand, SdkForgotPasswordResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ConsoleOptions _consoleOptions;

    public SdkForgotPasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IPasswordResetTokenService tokenService,
        IEmailService emailService,
        IOptions<ConsoleOptions> consoleOptions)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _tokenService = tokenService;
        _emailService = emailService;
        _consoleOptions = consoleOptions.Value;
    }

    public async Task<SdkForgotPasswordResponse> Handle(
        SdkForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // ------------------------------------------------------------
        // Current Application
        // ------------------------------------------------------------

        var applicationId = _currentApplication.ApplicationId;

        // ------------------------------------------------------------
        // Normalize Email
        // ------------------------------------------------------------

        var normalizedEmail = request.Email
            .Trim()
            .ToUpperInvariant();

        // ------------------------------------------------------------
        // Find Application User
        // ------------------------------------------------------------
        // The user must belong to the current application.
        // We never search by email globally.
        // ------------------------------------------------------------

        var user = await _dbContext.ApplicationUsers
            .Include(x => x.Account)
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.Account.NormalizedEmail == normalizedEmail &&
                    x.IsActive &&
                    x.Account.IsActive,
                cancellationToken);

        // ------------------------------------------------------------
        // Do not reveal whether the account exists
        // ------------------------------------------------------------

        if (user is null)
        {
            return new SdkForgotPasswordResponse(
                "If an account exists with this email, you will receive instructions to reset your password.");
        }

        // ------------------------------------------------------------
        // Revoke Existing Password Reset Tokens
        // ------------------------------------------------------------
        // Only one active reset flow should remain valid.
        // ------------------------------------------------------------

        var activeTokens =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Where(x =>
                    x.UserId == user.Id &&
                    x.UsedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var token in activeTokens)
        {
            token.UsedAt = now;
        }

        // ------------------------------------------------------------
        // Generate Password Reset Token
        // ------------------------------------------------------------

        var rawToken = _tokenService.GenerateToken();

        var tokenHash = _tokenService.HashToken(rawToken);

        var resetToken = new ApplicationUserPasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = now.AddMinutes(30)
        };

        _dbContext.ApplicationUserPasswordResetTokens.Add(resetToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // ------------------------------------------------------------
        // Build Reset URL
        // ------------------------------------------------------------

        var consoleUrl = _consoleOptions.BaseUrl.TrimEnd('/');

        var resetUrl =
            $"{consoleUrl}/sdk/reset-password?token={Uri.EscapeDataString(rawToken)}";

        // ------------------------------------------------------------
        // Send Email
        // ------------------------------------------------------------

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

        // ------------------------------------------------------------
        // Generic Response
        // ------------------------------------------------------------

        return new SdkForgotPasswordResponse(
            "If an account exists with this email, you will receive instructions to reset your password.");
    }
}