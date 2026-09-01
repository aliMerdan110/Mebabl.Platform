namespace Mebabl.Platform.Application.Common.Security;

public interface IDocumentSecurityService
{
    Task EnsureReadAsync(
        Guid collectionId,
        CancellationToken cancellationToken);

    Task EnsureWriteAsync(
        Guid collectionId,
        CancellationToken cancellationToken);

    Task EnsureDeleteAsync(
        Guid collectionId,
        CancellationToken cancellationToken);

    Task EnsureQueryAsync(
        Guid collectionId,
        CancellationToken cancellationToken);
}