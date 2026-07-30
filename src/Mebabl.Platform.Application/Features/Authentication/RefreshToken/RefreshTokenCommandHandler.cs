using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Authentication.DTOs;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Domain.Entities.Identity;
using RefreshTokenEntity = Mebabl.Platform.Domain.Entities.Identity.RefreshToken;

namespace Mebabl.Platform.Application.Features.Authentication.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IApplicationDbContext dbContext,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
    }


    public async Task<AuthResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _dbContext.RefreshTokens
    .Include(x => x.ApplicationUser)
        .ThenInclude(x => x.Account)
    .FirstOrDefaultAsync(
        x => x.Token == request.RefreshToken,
        cancellationToken);


        if (refreshToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }


        if (refreshToken.IsRevoked)
        {
            throw new UnauthorizedAccessException(
                "Refresh token has been revoked.");
        }


        if (refreshToken.IsExpired)
        {
            throw new UnauthorizedAccessException(
                "Refresh token has expired.");
        }


        // revoke old token
        refreshToken.RevokedAt = DateTime.UtcNow;


        // create new access token
        var user = refreshToken.ApplicationUser;

var accessToken = _jwtTokenGenerator.GenerateAccessToken(
    user.AccountId,
    user.Id,
    user.ApplicationId,
    user.Account.TenantId);


        // create new refresh token
        var newRefreshToken =
            new RefreshTokenEntity
            {
                ApplicationUserId =
                    refreshToken.ApplicationUserId,

                Token =
                    _jwtTokenGenerator.GenerateRefreshToken(),

                ExpiresAt =
                    DateTime.UtcNow.AddDays(30)
            };


        await _dbContext.RefreshTokens.AddAsync(
            newRefreshToken,
            cancellationToken);


        await _dbContext.SaveChangesAsync(
            cancellationToken);


        return new AuthResponse(
            refreshToken.ApplicationUserId,
            accessToken,
            newRefreshToken.Token);
    }
}