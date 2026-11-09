
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Developers.Register;

public sealed class RegisterDeveloperCommandHandler
    : IRequestHandler<RegisterDeveloperCommand, RegisterDeveloperResponse>
{
    private static readonly TimeSpan RefreshTokenLifetime =
        TimeSpan.FromDays(7);

    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterDeveloperCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<RegisterDeveloperResponse> Handle(
        RegisterDeveloperCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            request.Email.Length > 256 ||
            string.IsNullOrWhiteSpace(request.DisplayName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                "Display name, email, and password are required.");
        }

        var email = request.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();

        var exists = await _dbContext.Developers
            .AsNoTracking()
            .AnyAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "A developer with this email already exists.");
        }

        var developer = new Developer
        {
            DisplayName = request.DisplayName.Trim(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _dbContext.Developers.Add(developer);

        // Return the raw token to the client only.
        // Never persist the raw refresh token in the database.
        var now = DateTime.UtcNow;
        var rawRefreshToken =
            _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = new DeveloperRefreshToken
        {
            Developer = developer,
            Token = null,
            TokenHash = RefreshTokenHashing.Hash(rawRefreshToken),
            ExpiresAt = now.Add(RefreshTokenLifetime)
        };

        developer.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken =
            _jwtTokenGenerator.GenerateDeveloperToken(developer.Id);

        return new RegisterDeveloperResponse(
            accessToken,
            rawRefreshToken);
    }
}
