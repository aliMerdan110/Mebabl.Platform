using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Common.Services.ApplicationInitialization;

public sealed class ApplicationInitializer
    : IApplicationInitializer
{
    private readonly IApplicationDbContext _dbContext;

    public ApplicationInitializer(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var owner = new Role
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Name = "Owner",
            Description = "Application Owner"
        };

        var admin = new Role
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Name = "Admin",
            Description = "Application Administrator"
        };

        var user = new Role
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Name = "User",
            Description = "Application User"
        };

        _dbContext.Roles.AddRange(owner, admin, user);

        var permissions = new[]
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
            "credentials.disable"
        };

        var permissionEntities = permissions
            .Select(code => new Permission
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                Name = code,
                Code = code,
                Description = code
            })
            .ToList();

        _dbContext.Permissions.AddRange(permissionEntities);

        foreach (var permission in permissionEntities)
        {
            _dbContext.RolePermissions.Add(
                new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = owner.Id,
                    PermissionId = permission.Id
                });
        }

        foreach (var permission in permissionEntities.Where(x =>
                     x.Code.StartsWith("users.") ||
                     x.Code.StartsWith("roles.") ||
                     x.Code.StartsWith("permissions.")))
        {
            _dbContext.RolePermissions.Add(
                new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = admin.Id,
                    PermissionId = permission.Id
                });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}