using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Services.Authorization;

namespace Mebabl.Platform.Infrastructure.Authorization;

public sealed class PermissionChecker : IPermissionChecker
{
    private readonly IApplicationDbContext _dbContext;

    public PermissionChecker(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasPermissionAsync(
        Guid applicationId,
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        var code = permissionCode
            .Trim()
            .ToLowerInvariant();

        return _dbContext.ApplicationUserRoles
            .AnyAsync(
                x =>
                    x.ApplicationUserId == userId &&
                    x.Role.ApplicationId == applicationId &&
                    x.Role.RolePermissions.Any(
                        rp =>
                            rp.Permission.ApplicationId == applicationId &&
                            rp.Permission.Code == code &&
                            rp.Permission.IsActive),
                cancellationToken);
    }
}