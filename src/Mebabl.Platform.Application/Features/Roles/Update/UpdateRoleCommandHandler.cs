using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Roles.Update;

public sealed class UpdateRoleCommandHandler
    : IRequestHandler<UpdateRoleCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public UpdateRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (role is null)
            throw new Exception("Role not found.");

        var code = request.Code.Trim().ToUpperInvariant();

        var exists = await _dbContext.Roles.AnyAsync(
            x => x.Id != request.Id &&
                 x.ApplicationId == _currentApplication.ApplicationId &&
                 x.Code == code,
            cancellationToken);

        if (exists)
            throw new Exception("Role code already exists.");

        role.Name = request.Name.Trim();
        role.Code = code;
        role.Description = request.Description ?? string.Empty;
        role.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}