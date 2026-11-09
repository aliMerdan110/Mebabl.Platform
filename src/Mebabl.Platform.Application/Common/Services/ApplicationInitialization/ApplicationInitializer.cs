using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Common.Services.ApplicationInitialization;

public sealed class ApplicationInitializer : IApplicationInitializer
{
    private readonly IApplicationDbContext _dbContext;

    public ApplicationInitializer(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var roles = new[]
        {
            new Role
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                Name = "Owner",
                Code = "owner",
                Description = "Application Owner"
            },
            new Role
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                Name = "Admin",
                Code = "admin",
                Description = "Application Administrator"
            },
            new Role
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                Name = "User",
                Code = "user",
                Description = "Application User"
            }
        };

        _dbContext.Roles.AddRange(roles);

        var owner = roles[0];
        var admin = roles[1];

        var permissionCodes = new[]
        {
            "applications.read",
            "applications.update",
            "users.read",
            "users.create",
            "users.update",
            "users.delete",
            "roles.read",
            "roles.create",
            "roles.update",
            "roles.delete",
            "permissions.read",
            "permissions.create",
            "permissions.update",
            "permissions.delete",
            "credentials.read",
            "credentials.create",
            "credentials.enable",
            "credentials.disable",
            "live.view",
            "live.publish"
        };

        var permissions = permissionCodes
            .Select(code => new Permission
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                Name = code,
                Code = code,
                Description = code
            })
            .ToArray();

        _dbContext.Permissions.AddRange(permissions);

        foreach (var permission in permissions)
        {
            _dbContext.RolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = owner.Id,
                PermissionId = permission.Id
            });
        }

        foreach (var permission in permissions.Where(permission =>
                     permission.Code.StartsWith("users.", StringComparison.Ordinal) ||
                     permission.Code.StartsWith("roles.", StringComparison.Ordinal) ||
                     permission.Code.StartsWith("permissions.", StringComparison.Ordinal)))
        {
            _dbContext.RolePermissions.Add(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = admin.Id,
                PermissionId = permission.Id
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}