using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.PasswordReset;

namespace Mebabl.Platform.Application.Features.SdkAuth.VerifyEmail;

public sealed class VerifyEmailCommandHandler
    : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordResetTokenService _tokenService;

    public VerifyEmailCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordResetTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<VerifyEmailResponse> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.Token);
        var now = DateTime.UtcNow;

        var verificationToken =
            await _dbContext.ApplicationUserEmailVerificationTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x =>
                        x.TokenHash == tokenHash &&
                        x.UsedAt == null &&
                        x.ExpiresAt > now,
                    cancellationToken);

        if (verificationToken is null)
        {
            throw new UnauthorizedAccessException(
                "The email verification token is invalid or has expired.");
        }

        var user = verificationToken.User;

        if (user.IsDeleted || !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "The user account is inactive.");
        }

        user.EmailConfirmed = true;
        user.UpdatedAt = now;

        verificationToken.UsedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponse(
            "Email has been verified successfully.");
    }
}