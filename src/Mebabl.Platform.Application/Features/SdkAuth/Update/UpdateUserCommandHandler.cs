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

        var user = await _dbContext.ApplicationUsers
            .Include(x => x.Account)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (user is null)
            throw new Exception("User not found.");

        user.Account.Username = request.Username;
        user.Account.NormalizedUsername =
            request.Username.Trim().ToUpperInvariant();

        user.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}