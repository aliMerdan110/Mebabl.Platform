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

var verificationToken =
    await _dbContext.ApplicationUserEmailVerificationTokens
        .Include(x => x.User)
        .ThenInclude(x => x.Account)
        .FirstOrDefaultAsync(
            x =>
                x.TokenHash == tokenHash &&
                x.UsedAt == null &&
                x.ExpiresAt > DateTime.UtcNow,
            cancellationToken);

        if (verificationToken is null)
        {
            throw new UnauthorizedAccessException(
                "The email verification token is invalid or has expired.");
        }

        var user = verificationToken.User;

        if (!user.IsActive || !user.Account.IsActive)
        {
            throw new UnauthorizedAccessException(
                "The user account is inactive.");
        }

        user.Account.EmailConfirmed = true;

        verificationToken.UsedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResponse(
            "Email has been verified successfully.");
    }
}