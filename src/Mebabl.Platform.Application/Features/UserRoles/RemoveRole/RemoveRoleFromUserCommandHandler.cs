using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.UserRoles.RemoveRole;

public sealed class RemoveRoleFromUserCommandHandler
    : IRequestHandler<RemoveRoleFromUserCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveRoleFromUserCommandHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(
        RemoveRoleFromUserCommand request,
        CancellationToken cancellationToken)
    {
        var userRole = await _dbContext.ApplicationUserRoles
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationUserId == request.UserId &&
                    x.RoleId == request.RoleId,
                cancellationToken);

        if (userRole is null)
            throw new Exception("Role is not assigned to user.");

        _dbContext.ApplicationUserRoles.Remove(userRole);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}