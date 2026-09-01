using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Developers.RefreshToken;

public sealed class RefreshDeveloperTokenCommandHandler
    : IRequestHandler<RefreshDeveloperTokenCommand, RefreshDeveloperTokenResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshDeveloperTokenCommandHandler(
        IApplicationDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<RefreshDeveloperTokenResponse> Handle(
        RefreshDeveloperTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _dbContext.DeveloperRefreshTokens
            .Include(x => x.Developer)
            .FirstOrDefaultAsync(
                x => x.Token == request.RefreshToken,
                cancellationToken);

       
if (refreshToken is null)
    throw new UnauthorizedAccessException("Invalid refresh token.");

if (refreshToken.IsRevoked)
    throw new UnauthorizedAccessException("Refresh token has been revoked.");

if (refreshToken.IsExpired)
    throw new UnauthorizedAccessException("Refresh token has expired.");


        refreshToken.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = new DeveloperRefreshToken
        {
            DeveloperId = refreshToken.DeveloperId,
            Token = _jwtTokenGenerator.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.DeveloperRefreshTokens.Add(newRefreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken =
            _jwtTokenGenerator.GenerateDeveloperToken(
                refreshToken.DeveloperId);

        return new RefreshDeveloperTokenResponse(
            accessToken,
            newRefreshToken.Token);
    }
}