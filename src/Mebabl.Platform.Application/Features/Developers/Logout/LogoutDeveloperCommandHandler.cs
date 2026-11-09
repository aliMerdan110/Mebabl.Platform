using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;

namespace Mebabl.Platform.Application.Features.Developers.Logout;

public sealed class LogoutDeveloperCommandHandler
    : IRequestHandler<LogoutDeveloperCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public LogoutDeveloperCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        LogoutDeveloperCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated ||
            _currentDeveloper.DeveloperId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated developer is required.");
        }

        var rawToken = request.RefreshToken;

        if (string.IsNullOrWhiteSpace(rawToken) ||
            rawToken.Length > 512)
        {
            return;
        }

        var tokenHash = RefreshTokenHashing.Hash(rawToken);
        var developerId = _currentDeveloper.DeveloperId;

        var token = await _dbContext.DeveloperRefreshTokens
            .FirstOrDefaultAsync(
                x =>
                    x.DeveloperId == developerId &&
                    (x.TokenHash == tokenHash ||
                     x.Token == rawToken),
                cancellationToken);

        if (token is null)
            return;

        token.RevokedAt ??= DateTime.UtcNow;

        // ترحيل هذا الرمز القديم إلى التجزئة عند إلغائه.
        token.TokenHash ??= tokenHash;
        token.Token = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}