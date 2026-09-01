using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Permissions.Create;

public sealed class CreatePermissionCommandHandler
    : IRequestHandler<CreatePermissionCommand, CreatePermissionResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public CreatePermissionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<CreatePermissionResponse> Handle(
        CreatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var code = request.Code.Trim().ToLowerInvariant();

        var exists = await _dbContext.Permissions
            .AnyAsync(
                x => x.ApplicationId == _currentApplication.ApplicationId &&
                     x.Code == code,
                cancellationToken);

        if (exists)
            throw new Exception("Permission code already exists.");

        var permission = new Permission
        {
            ApplicationId = _currentApplication.ApplicationId,
            Name = request.Name.Trim(),
            Code = code,
            Description = request.Description ?? string.Empty,
            Category = request.Category,
            IsActive = true
        };

        _dbContext.Permissions.Add(permission);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreatePermissionResponse(
            permission.Id,
            permission.Name,
            permission.Code);
    }
}