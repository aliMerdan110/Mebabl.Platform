namespace Mebabl.Platform.Application.Common.Security;

/// <summary>
/// Enforces the published security policy for project-owned database collections.
/// The implementation must derive the project scope from the authenticated
/// request context; callers must not be able to supply or override that scope.
/// Access is denied by default.
/// </summary>
public interface IDocumentSecurityService
{
    /// <summary>
    /// Ensures that the current authenticated principal may read documents
    /// from the specified collection in the current project.
    /// </summary>
    /// <param name="collectionId">The collection identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// The principal is not authenticated, the collection is outside the
    /// current project, or no active published policy grants read access.
    /// </exception>
    Task EnsureReadAsync(
        Guid collectionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ensures that the current authenticated principal may create or update
    /// documents in the specified collection in the current project.
    /// </summary>
    /// <param name="collectionId">The collection identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// The principal is not authenticated, the collection is outside the
    /// current project, or no active published policy grants write access.
    /// </exception>
    Task EnsureWriteAsync(
        Guid collectionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ensures that the current authenticated principal may delete documents
    /// from the specified collection in the current project.
    /// </summary>
    /// <param name="collectionId">The collection identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// The principal is not authenticated, the collection is outside the
    /// current project, or no active published policy grants delete access.
    /// </exception>
    Task EnsureDeleteAsync(
        Guid collectionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ensures that the current authenticated principal may execute a query
    /// against the specified collection in the current project. Query access
    /// does not replace the separate read-access check on returned documents.
    /// </summary>
    /// <param name="collectionId">The collection identifier.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <exception cref="UnauthorizedAccessException">
    /// The principal is not authenticated, the collection is outside the
    /// current project, or no active published policy grants query access.
    /// </exception>
    Task EnsureQueryAsync(
        Guid collectionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns the intersection of the supplied collection identifiers and
    /// the collections in the current authenticated project whose active,
    /// published policies grant read access.
    /// </summary>
    /// <remarks>
    /// Unknown, inactive, deleted, unpublished, malformed, unsupported, or
    /// cross-project collections must be excluded. The method must not treat
    /// requested identifiers as proof of authorization and should evaluate
    /// the set in a bounded number of database queries.
    /// </remarks>
    /// <param name="collectionIds">Candidate collection identifiers.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>
    /// A set containing only identifiers for which read access is granted.
    /// </returns>
    Task<IReadOnlySet<Guid>> GetReadableCollectionIdsAsync(
        IReadOnlyCollection<Guid> collectionIds,
        CancellationToken cancellationToken);
}
