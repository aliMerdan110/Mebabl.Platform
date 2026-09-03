// Infrastructure/Live/LiveAuthorizationService.cs

using Mebabl.Platform.Application.Common.Services.Authorization;
using Mebabl.Platform.Application.Services.Live;

namespace Mebabl.Platform.Infrastructure.Live;

public sealed class LiveAuthorizationService : ILiveAuthorizationService
{
    private readonly IPermissionChecker _permissionChecker;

    public LiveAuthorizationService(
        IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public Task<bool> CanPublishAsync(
        Guid applicationId,
        Guid userId,
        Guid streamId,
        CancellationToken cancellationToken = default)
    {
        // صلاحية المستخدم تأتي من Application.
        // Mebabl لا يفترض أن Developer هو broadcaster.
        return _permissionChecker.HasPermissionAsync(
            applicationId,
            userId,
            "live.publish",
            cancellationToken);
    }

    public Task<bool> CanViewAsync(
        Guid applicationId,
        Guid userId,
        Guid streamId,
        CancellationToken cancellationToken = default)
    {
        return _permissionChecker.HasPermissionAsync(
            applicationId,
            userId,
            "live.view",
            cancellationToken);
    }
}
