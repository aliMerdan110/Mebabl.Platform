
using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Jwt;

namespace Mebabl.Platform.Application.Features.Developers.RefreshToken;

public sealed class RefreshDeveloperTokenCommandHandler
    : IRequestHandler<
        RefreshDeveloperTokenCommand,
        RefreshDeveloperTokenResponse>
{
    private static readonly TimeSpan RefreshTokenLifetime =
        TimeSpan.FromDays(7);

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
        if (request is null ||
            string.IsNullOrWhiteSpace(request.RefreshToken) ||
            request.RefreshToken.Length > 512)
        {
            throw new UnauthorizedAccessException(
                "Invalid, expired, or revoked refresh token.");
        }

        var now = DateTime.UtcNow;
        var currentToken = request.RefreshToken;
        var currentTokenHash = RefreshTokenHashing.Hash(currentToken);

        // Generate a new refresh token and keep only its hash in storage.
        var newRawRefreshToken =
            _jwtTokenGenerator.GenerateRefreshToken();

        var newRefreshTokenHash =
            RefreshTokenHashing.Hash(newRawRefreshToken);

        // This operation must atomically validate and revoke the
        // current token, then persist the replacement token.
        // It also supports legacy plaintext tokens during migration.
        var developerId =
            await _dbContext.RotateDeveloperRefreshTokenAsync(
                currentToken,
                currentTokenHash,
                newRefreshTokenHash,
                now,
                now.Add(RefreshTokenLifetime),
                cancellationToken);

        if (!developerId.HasValue ||
            developerId.Value == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Invalid, expired, or revoked refresh token.");
        }

        var accessToken =
            _jwtTokenGenerator.GenerateDeveloperToken(
                developerId.Value);

        return new RefreshDeveloperTokenResponse(
            accessToken,
            newRawRefreshToken);
    }
}
