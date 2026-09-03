namespace Mebabl.Platform.Application.Common.Services.Authorization;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(
        Guid applicationId,
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default);
}