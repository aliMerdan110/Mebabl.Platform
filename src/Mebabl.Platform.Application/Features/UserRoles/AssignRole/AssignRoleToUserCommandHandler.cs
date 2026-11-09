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
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application is required.");
        }

        if (request.UserId == Guid.Empty ||
            request.RoleId == Guid.Empty)
        {
            throw new ArgumentException(
                "Valid user and role IDs are required.");
        }

        var userExists = await _dbContext.ApplicationUsers
            .AnyAsync(
                x => x.Id == request.UserId &&
                     x.ApplicationId == applicationId,
                cancellationToken);

        if (!userExists)
            throw new KeyNotFoundException("User not found.");

        var roleExists = await _dbContext.Roles
            .AnyAsync(
                x => x.Id == request.RoleId &&
                     x.ApplicationId == applicationId,
                cancellationToken);

        if (!roleExists)
            throw new KeyNotFoundException("Role not found.");

        var exists = await _dbContext.ApplicationUserRoles
            .AnyAsync(
                x => x.ApplicationUserId == request.UserId &&
                     x.RoleId == request.RoleId,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "Role is already assigned to this user.");
        }

        _dbContext.ApplicationUserRoles.Add(
            new ApplicationUserRole
            {
                ApplicationUserId = request.UserId,
                RoleId = request.RoleId
            });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}