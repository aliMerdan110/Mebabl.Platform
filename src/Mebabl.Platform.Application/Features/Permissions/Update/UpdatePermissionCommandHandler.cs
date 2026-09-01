using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Permissions.Update;

public sealed class UpdatePermissionCommandHandler
    : IRequestHandler<UpdatePermissionCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public UpdatePermissionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        UpdatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var permission = await _dbContext.Permissions
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (permission is null)
            throw new Exception("Permission not found.");

        var code = request.Code
            .Trim()
            .ToLowerInvariant();

        var exists = await _dbContext.Permissions
            .AnyAsync(
                x =>
                    x.Id != request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    x.Code == code,
                cancellationToken);

        if (exists)
            throw new Exception("Permission code already exists.");

        permission.Name = request.Name.Trim();
        permission.Code = code;
        permission.Description = request.Description ?? string.Empty;
        permission.Category = request.Category;
        permission.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}