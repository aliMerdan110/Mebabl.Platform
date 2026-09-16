using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Password;

namespace Mebabl.Platform.Application.Features.SdkAuth.ChangePassword;

public sealed class SdkChangePasswordCommandHandler
    : IRequestHandler<SdkChangePasswordCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;

    public SdkChangePasswordCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(
        SdkChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var applicationId = _currentUser.ApplicationId;

        if (applicationId == Guid.Empty ||
            _currentUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var user = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                x =>
                    x.Id == _currentUser.UserId &&
                    x.ApplicationId == applicationId &&
                    x.IsActive &&
                    !x.IsDeleted,
                cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException();

        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");

        var currentPasswordValid =
            _passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash);

        if (!currentPasswordValid)
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");

        user.PasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        user.SecurityStamp = Guid.NewGuid().ToString();
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}