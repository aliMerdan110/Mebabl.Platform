using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Email;
using Mebabl.Platform.Application.Services.PasswordReset;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.SdkAuth.ResendVerificationEmail;

public sealed class ResendVerificationEmailCommandHandler
    : IRequestHandler<
        ResendVerificationEmailCommand,
        ResendVerificationEmailResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly IEmailService _emailService;

    public ResendVerificationEmailCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IPasswordResetTokenService tokenService,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<ResendVerificationEmailResponse> Handle(
        ResendVerificationEmailCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                x =>
                    x.Id == _currentUser.UserId &&
                    x.ApplicationId == _currentUser.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "The user account is inactive.");
        }

        if (user.EmailConfirmed)
        {
            return new ResendVerificationEmailResponse(
                "Email is already verified.");
        }

        var now = DateTime.UtcNow;

        var activeTokens =
            await _dbContext.ApplicationUserEmailVerificationTokens
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

        var verificationToken =
            new ApplicationUserEmailVerificationToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = now.AddMinutes(30)
            };

        _dbContext.ApplicationUserEmailVerificationTokens
            .Add(verificationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var verificationUrl =
            $"https://mebabl.com/sdk/verify-email?token=" +
            Uri.EscapeDataString(rawToken);

        await _emailService.SendAsync(
            user.Email,
            "Verify your email",
            $"""
            Hello,

            Please verify your email address using the following link:

            {verificationUrl}

            This link will expire in 30 minutes.

            If you did not request this email, you can safely ignore it.

            Mebabl Platform
            """,
            cancellationToken);

        return new ResendVerificationEmailResponse(
            "Verification email has been sent.");
    }
}