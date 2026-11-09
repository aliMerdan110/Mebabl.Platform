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
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application is required.");
        }

        if (request.RoleId == Guid.Empty ||
            request.PermissionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Valid role and permission IDs are required.");
        }

        var roleExists = await _dbContext.Roles
            .AnyAsync(
                x => x.Id == request.RoleId &&
                     x.ApplicationId == applicationId,
                cancellationToken);

        if (!roleExists)
            throw new KeyNotFoundException("Role not found.");

        var permissionExists = await _dbContext.Permissions
            .AnyAsync(
                x => x.Id == request.PermissionId &&
                     x.ApplicationId == applicationId,
                cancellationToken);

        if (!permissionExists)
            throw new KeyNotFoundException("Permission not found.");

        var exists = await _dbContext.RolePermissions
            .AnyAsync(
                x => x.RoleId == request.RoleId &&
                     x.PermissionId == request.PermissionId,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "Permission is already assigned to this role.");
        }

        _dbContext.RolePermissions.Add(
            new RolePermission
            {
                RoleId = request.RoleId,
                PermissionId = request.PermissionId
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}