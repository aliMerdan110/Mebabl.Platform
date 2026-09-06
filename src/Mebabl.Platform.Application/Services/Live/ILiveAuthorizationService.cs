namespace Mebabl.Platform.Application.Services.Live;

public interface ILiveAuthorizationService
{
    Task<bool> CanPublishAsync(
        Guid applicationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> CanViewAsync(
        Guid applicationId,
        Guid userId,
        Guid streamId,
        CancellationToken cancellationToken = default);
}
