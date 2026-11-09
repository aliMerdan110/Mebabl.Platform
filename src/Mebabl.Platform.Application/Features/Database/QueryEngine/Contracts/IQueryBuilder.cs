using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

/// <summary>
/// Builds provider-specific, server-translated document queries. Provider-only
/// operations such as PostgreSQL row-value cursor comparisons belong in the
/// implementation, not in the Application layer.
/// </summary>
public interface IQueryBuilder
{
    IQueryable<Document> Apply(
        IQueryable<Document> query,
        QueryRequest request);

    /// <summary>
    /// Restricts a descending chronological query to rows strictly after the
    /// supplied (CreatedAt, Id) cursor position.
    /// Implementations must translate this predicate on the database server.
    /// </summary>
    IQueryable<Document> ApplyCursor(
        IQueryable<Document> query,
        DateTime createdAtUtc,
        Guid documentId);
}
