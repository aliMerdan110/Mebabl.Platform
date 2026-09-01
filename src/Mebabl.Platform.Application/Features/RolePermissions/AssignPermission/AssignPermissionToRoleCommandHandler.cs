using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.RolePermissions.AssignPermission;

public sealed class AssignPermissionToRoleCommandHandler
    : IRequestHandler<AssignPermissionToRoleCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public AssignPermissionToRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        AssignPermissionToRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var role = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.RoleId &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (role is null)
            throw new Exception("Role not found.");

        var permission = await _dbContext.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.PermissionId &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (permission is null)
            throw new Exception("Permission not found.");

        var exists = await _dbContext.RolePermissions
            .AnyAsync(
                x =>
                    x.RoleId == request.RoleId &&
                    x.PermissionId == request.PermissionId,
                cancellationToken);

        if (exists)
            throw new Exception("Permission already assigned to role.");

        _dbContext.RolePermissions.Add(new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = request.PermissionId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}