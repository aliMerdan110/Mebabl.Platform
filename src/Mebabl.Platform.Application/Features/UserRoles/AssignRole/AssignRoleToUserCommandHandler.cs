using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.UserRoles.AssignRole;

public sealed class AssignRoleToUserCommandHandler
    : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public AssignRoleToUserCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        AssignRoleToUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var user = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.UserId &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (user is null)
            throw new Exception("User not found.");

        var role = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.RoleId &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (role is null)
            throw new Exception("Role not found.");

        var exists = await _dbContext.ApplicationUserRoles
            .AnyAsync(
                x =>
                    x.ApplicationUserId == request.UserId &&
                    x.RoleId == request.RoleId,
                cancellationToken);

        if (exists)
            throw new Exception("Role already assigned to user.");

        _dbContext.ApplicationUserRoles.Add(new ApplicationUserRole
        {
            ApplicationUserId = request.UserId,
            RoleId = request.RoleId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}