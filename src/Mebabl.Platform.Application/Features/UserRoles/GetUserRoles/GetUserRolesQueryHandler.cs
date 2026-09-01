using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.UserRoles.GetUserRoles;

public sealed class GetUserRolesQueryHandler
    : IRequestHandler<GetUserRolesQuery, IReadOnlyList<UserRoleItem>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public GetUserRolesQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<UserRoleItem>> Handle(
        GetUserRolesQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var userExists = await _dbContext.ApplicationUsers
            .AnyAsync(
                x =>
                    x.Id == request.UserId &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (!userExists)
            throw new Exception("User not found.");

        return await _dbContext.ApplicationUserRoles
            .AsNoTracking()
            .Where(x => x.ApplicationUserId == request.UserId)
            .Select(x => new UserRoleItem(
                x.Role.Id,
                x.Role.Name,
                x.Role.Code,
                x.Role.Description,
                x.Role.IsActive))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}