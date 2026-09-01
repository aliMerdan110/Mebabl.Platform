using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Developers.Logout;

public sealed class LogoutDeveloperCommandHandler
    : IRequestHandler<LogoutDeveloperCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public LogoutDeveloperCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        LogoutDeveloperCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _dbContext.DeveloperRefreshTokens
            .FirstOrDefaultAsync(
                x => x.Token == request.RefreshToken,
                cancellationToken);

        if (refreshToken is null)
            return;

        refreshToken.RevokedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}