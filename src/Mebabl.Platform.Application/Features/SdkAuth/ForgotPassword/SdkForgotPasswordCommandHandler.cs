using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Options;
using Mebabl.Platform.Application.Services.Email;
using Mebabl.Platform.Application.Services.PasswordReset;

namespace Mebabl.Platform.Application.Features.SdkAuth.ForgotPassword;

public sealed class SdkForgotPasswordCommandHandler
    : IRequestHandler<SdkForgotPasswordCommand, SdkForgotPasswordResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly AuthOptions _authOptions;

    public SdkForgotPasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IPasswordResetTokenService tokenService,
        IEmailService emailService,
        IOptions<AuthOptions> authOptions)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _tokenService = tokenService;
        _emailService = emailService;
        _authOptions = authOptions.Value;
    }

    public async Task<SdkForgotPasswordResponse> Handle(
        SdkForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        if (applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Application authentication is required.");
        }

        var normalizedEmail = request.Email
            .Trim()
            .ToUpperInvariant();

        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.NormalizedEmail == normalizedEmail &&
                    x.IsActive &&
                    !x.IsDeleted,
                cancellationToken);

        const string genericMessage =
            "If an account exists with this email, you will receive instructions to reset your password.";

        if (user is null)
        {
            return new SdkForgotPasswordResponse(genericMessage);
        }

        var now = DateTime.UtcNow;

        var activeTokens =
            await _dbContext.ApplicationUserPasswordResetTokens
                .Where(
                    x =>
                        x.UserId == user.Id &&
                        x.UsedAt == null &&
                        x.ExpiresAt > now)
                .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.UsedAt = now;
        }

        var rawToken = _tokenService.GenerateToken();
        var tokenHash = _tokenService.HashToken(rawToken);

        var resetToken =
            new Domain.Entities.Identity.ApplicationUserPasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = now.AddMinutes(30)
            };

        _dbContext.ApplicationUserPasswordResetTokens.Add(resetToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var authUrl = _authOptions.BaseUrl.TrimEnd('/');

        var resetUrl =
            $"{authUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

        await _emailService.SendAsync(
            user.Email,
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

        return new SdkForgotPasswordResponse(genericMessage);
    }
}