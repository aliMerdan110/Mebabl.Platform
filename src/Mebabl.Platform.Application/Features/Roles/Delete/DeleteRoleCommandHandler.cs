using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Roles.Delete;

public sealed class DeleteRoleCommandHandler
    : IRequestHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public DeleteRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var role = await _dbContext.Roles
            .Include(x => x.UserRoles)
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (role is null)
            throw new Exception("Role not found.");

        if (role.UserRoles.Any())
            throw new Exception("Role is assigned to users.");

        if (role.RolePermissions.Any())
            throw new Exception("Role contains permissions.");

        _dbContext.Roles.Remove(role);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}