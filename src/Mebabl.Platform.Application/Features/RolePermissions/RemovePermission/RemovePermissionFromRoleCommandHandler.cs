using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.RolePermissions.RemovePermission;

public sealed class RemovePermissionFromRoleCommandHandler
    : IRequestHandler<RemovePermissionFromRoleCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public RemovePermissionFromRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        RemovePermissionFromRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var rolePermission = await _dbContext.RolePermissions
            .FirstOrDefaultAsync(
                x =>
                    x.RoleId == request.RoleId &&
                    x.PermissionId == request.PermissionId,
                cancellationToken);

        if (rolePermission is null)
            throw new Exception("Permission is not assigned to this role.");

        _dbContext.RolePermissions.Remove(rolePermission);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}