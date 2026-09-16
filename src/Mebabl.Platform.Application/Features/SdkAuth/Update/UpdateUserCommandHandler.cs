using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkAuth.Update;

public sealed class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public UpdateUserCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var applicationId = _currentApplication.ApplicationId;

        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (user is null)
            throw new Exception("User not found.");

        var username = request.Username.Trim();
        var normalizedUsername = username.ToUpperInvariant();

        var usernameExists = await _dbContext.ApplicationUsers
            .AnyAsync(
                x =>
                    x.Id != user.Id &&
                    x.ApplicationId == applicationId &&
                    x.NormalizedUsername == normalizedUsername &&
                    !x.IsDeleted,
                cancellationToken);

        if (usernameExists)
            throw new Exception(
                "Username is already in use in this application.");

        user.Username = username;
        user.NormalizedUsername = normalizedUsername;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}