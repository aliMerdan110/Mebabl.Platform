using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Developers.Login;

public sealed class LoginDeveloperCommandHandler
    : IRequestHandler<LoginDeveloperCommand, LoginDeveloperResponse>
{
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
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        var developer = await _dbContext.Developers
            .FirstOrDefaultAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (developer is null)
            throw new Exception("Invalid email or password.");

        if (!developer.IsActive)
            throw new Exception("Developer account is disabled.");

        var validPassword = _passwordHasher.Verify(
            request.Password,
            developer.PasswordHash);

        if (!validPassword)
            throw new Exception("Invalid email or password.");

        var refreshToken = new DeveloperRefreshToken
        {
            DeveloperId = developer.Id,
            Token = _jwtTokenGenerator.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.DeveloperRefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken =
            _jwtTokenGenerator.GenerateDeveloperToken(developer.Id);

        return new LoginDeveloperResponse(
            accessToken,
            refreshToken.Token);
    }
}