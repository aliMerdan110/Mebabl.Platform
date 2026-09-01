using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.RolePermissions.GetRolePermissions;

public sealed class GetRolePermissionsQueryHandler
    : IRequestHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionItem>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public GetRolePermissionsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<RolePermissionItem>> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var roleExists = await _dbContext.Roles
            .AnyAsync(
                x =>
                    x.Id == request.RoleId &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (!roleExists)
            throw new Exception("Role not found.");

        return await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == request.RoleId)
            .Select(x => new RolePermissionItem(
                x.Permission.Id,
                x.Permission.Name,
                x.Permission.Code,
                x.Permission.Description,
                x.Permission.Category,
                x.Permission.IsActive))
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}