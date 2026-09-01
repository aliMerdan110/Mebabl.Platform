using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Permissions.Delete;

public sealed class DeletePermissionCommandHandler
    : IRequestHandler<DeletePermissionCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public DeletePermissionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        DeletePermissionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var permission = await _dbContext.Permissions
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (permission is null)
            throw new Exception("Permission not found.");

        if (permission.RolePermissions.Any())
            throw new Exception(
                "Permission is assigned to one or more roles.");

        _dbContext.Permissions.Remove(permission);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}