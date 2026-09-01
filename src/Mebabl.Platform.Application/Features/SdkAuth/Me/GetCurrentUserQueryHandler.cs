using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkAuth.Me;

public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CurrentUserResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _db.ApplicationUsers
            .Include(x => x.Account)
            .FirstAsync(
                x => x.Id == _currentUser.UserId,
                cancellationToken);

        var roles = await _db.ApplicationUserRoles
            .Where(x => x.ApplicationUserId == user.Id)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions = await _db.ApplicationUserRoles
            .Where(x => x.ApplicationUserId == user.Id)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new CurrentUserResponse(
            user.AccountId,
            user.Id,
            user.ApplicationId,
            user.Account.Email,
            user.Account.Username,
            roles,
            permissions);
    }
}