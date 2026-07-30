using Microsoft.EntityFrameworkCore;
using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Authentication.DTOs;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;
using RefreshTokenEntity = Mebabl.Platform.Domain.Entities.Identity.RefreshToken;

namespace Mebabl.Platform.Application.Features.Authentication.Login;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(
                x => x.NormalizedEmail ==
                     request.Email.Trim().ToUpperInvariant(),
                cancellationToken);

        if (account is null)
        {
            throw new Exception("Invalid email or password");
        }

        var passwordIsValid = _passwordHasher.Verify(
            request.Password,
            account.PasswordHash);

        if (!passwordIsValid)
        {
            throw new Exception("Invalid email or password");
        }

        if (!account.IsActive)
        {
            throw new Exception("Account is disabled");
        }

       var applicationUser = await _dbContext.ApplicationUsers
    .Include(x => x.Account)
    .FirstOrDefaultAsync(
        x => x.AccountId == account.Id &&
             x.ApplicationId == request.ApplicationId,
        cancellationToken);

if (applicationUser is null)
{
    throw new Exception("User is not registered for this application");
}

if (!applicationUser.IsActive)
{
    throw new Exception("User is disabled for this application");
}

var accessToken = _jwtTokenGenerator.GenerateAccessToken(
    account.Id,
    applicationUser.Id,
    applicationUser.ApplicationId,
    account.TenantId);

var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

var refreshTokenEntity = new RefreshTokenEntity
{
    ApplicationUserId = applicationUser.Id,
    Token = refreshToken,
    ExpiresAt = DateTime.UtcNow.AddDays(30)
};

_dbContext.RefreshTokens.Add(refreshTokenEntity);

await _dbContext.SaveChangesAsync(cancellationToken);

return new AuthResponse(
    applicationUser.ApplicationId,
    accessToken,
    refreshToken);
    }
}