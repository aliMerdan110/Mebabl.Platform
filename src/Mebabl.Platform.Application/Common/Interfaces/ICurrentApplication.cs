namespace Mebabl.Platform.Application.Common.Interfaces;

public interface ICurrentApplication
{
    Guid ApplicationId { get; }

    bool IsAuthenticated { get; }

    Task<bool> ValidateAsync(
        CancellationToken cancellationToken = default);
}