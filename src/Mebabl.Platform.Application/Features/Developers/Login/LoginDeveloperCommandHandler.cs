
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Developers.Login;

public sealed class LoginDeveloperCommandHandler
    : IRequestHandler<LoginDeveloperCommand, LoginDeveloperResponse>
{
    private static readonly TimeSpan RefreshTokenLifetime =
        TimeSpan.FromDays(7);

    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginDeveloperCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginDeveloperResponse> Handle(
        LoginDeveloperCommand request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Email) ||
            request.Email.Length > 256 ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var normalizedEmail =
            request.Email.Trim().ToUpperInvariant();

        var developer = await _dbContext.Developers
            .FirstOrDefaultAsync(
                x =>
                    x.NormalizedEmail == normalizedEmail &&
                    !x.IsDeleted,
                cancellationToken);

        if (developer is null ||
            !developer.IsActive ||
            string.IsNullOrWhiteSpace(developer.PasswordHash) ||
            !_passwordHasher.Verify(
                request.Password,
                developer.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var now = DateTime.UtcNow;

        // Generate the raw refresh token for the client.
        var rawRefreshToken =
            _jwtTokenGenerator.GenerateRefreshToken();

        // Persist only the SHA-256 hash, never the raw token.
        var refreshToken = new DeveloperRefreshToken
        {
            DeveloperId = developer.Id,
            Token = null,
            TokenHash = RefreshTokenHashing.Hash(rawRefreshToken),
            ExpiresAt = now.Add(RefreshTokenLifetime)
        };

        _dbContext.DeveloperRefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken =
            _jwtTokenGenerator.GenerateDeveloperToken(developer.Id);

        return new LoginDeveloperResponse(
            accessToken,
            rawRefreshToken);
    }
}
