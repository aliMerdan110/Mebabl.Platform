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

        // بما أن AccountId من نوع Guid عادي، تأكد مما إذا كان فارغاً (Guid.Empty)
        if (_currentUser.AccountId == Guid.Empty)
            throw new UnauthorizedAccessException();

        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(
                x => x.Id == _currentUser.AccountId && // <--- استخدمه مباشرة بدون .Value
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        if (account is null)
            throw new UnauthorizedAccessException();

        var currentPasswordValid =
            _passwordHasher.Verify(
                request.CurrentPassword,
                account.PasswordHash);

        if (!currentPasswordValid)
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");

        account.PasswordHash =
            _passwordHasher.Hash(request.NewPassword);

        account.SecurityStamp = Guid.NewGuid().ToString();

        account.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}