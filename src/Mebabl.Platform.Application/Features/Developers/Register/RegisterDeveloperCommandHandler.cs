using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Developers.Register;

public sealed class RegisterDeveloperCommandHandler
    : IRequestHandler<RegisterDeveloperCommand, RegisterDeveloperResponse>
{
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
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        var exists = await _dbContext.Developers.AnyAsync(
            x => x.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (exists)
            throw new Exception("Developer already exists.");

        var developer = new Developer
        {
            DisplayName = request.DisplayName,
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _dbContext.Developers.Add(developer);

        var refreshToken = new DeveloperRefreshToken
        {
            Developer = developer,
            Token = _jwtTokenGenerator.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        developer.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken =
            _jwtTokenGenerator.GenerateDeveloperToken(developer.Id);

        return new RegisterDeveloperResponse(
            accessToken,
            refreshToken.Token);
    }
}