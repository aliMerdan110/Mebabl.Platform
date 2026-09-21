using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Services.Authorization;

namespace Mebabl.Platform.Infrastructure.Authorization;

public sealed class PermissionChecker : IPermissionChecker
{
    private readonly IApplicationDbContext _dbContext;

    public PermissionChecker(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasPermissionAsync(
        Guid applicationId,
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        // يتحقق من الصلاحية ضمن نفس التطبيق وبحالة المستخدم والدور والصلاحية.
        if (applicationId == Guid.Empty ||
            userId == Guid.Empty ||
            string.IsNullOrWhiteSpace(permissionCode))
        {
            return Task.FromResult(false);
        }

        var code = permissionCode.Trim();

        return _dbContext.ApplicationUserRoles
            .AsNoTracking()
            .AnyAsync(
                userRole =>
                    userRole.ApplicationUserId == userId &&
                    userRole.ApplicationUser.ApplicationId == applicationId &&
                    userRole.ApplicationUser.IsActive &&
                    !userRole.ApplicationUser.IsDeleted &&
                    userRole.Role.ApplicationId == applicationId &&
                    userRole.Role.IsActive &&
                    !userRole.Role.IsDeleted &&
                    userRole.Role.RolePermissions.Any(
                        rolePermission =>
                            rolePermission.Permission.ApplicationId ==
                                applicationId &&
                            rolePermission.Permission.IsActive &&
                            !rolePermission.Permission.IsDeleted &&
                            rolePermission.Permission.Code == code),
                cancellationToken);
    }
}