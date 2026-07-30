using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Authentication.DTOs;
using Mebabl.Platform.Application.Services.CurrentUser;

namespace Mebabl.Platform.Application.Features.Authentication.Me;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<CurrentUserResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is null ||
            _currentUser.ApplicationId is null)
        {
            throw new UnauthorizedAccessException();
        }

        var user = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .Include(x => x.Account)
            .FirstOrDefaultAsync(
                x => x.Id == _currentUser.UserId &&
                     x.ApplicationId == _currentUser.ApplicationId,
                cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException();

        return new CurrentUserResponse(
            TenantId: _currentUser.TenantId ?? Guid.Empty,
            ApplicationId: user.ApplicationId,
            AccountId: user.AccountId,
            UserId: user.Id,
            Email: user.Account.Email
        );
    }
}