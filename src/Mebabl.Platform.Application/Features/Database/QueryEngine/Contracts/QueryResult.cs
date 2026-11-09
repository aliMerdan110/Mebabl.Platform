using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

/// <summary>
/// Represents one document returned by the database query engine.
/// The version is the optimistic-concurrency value required by subsequent
/// update or delete operations.
/// </summary>
public sealed record QueryResult(
    Guid Id,
    string Key,
    JsonDocument Data,
    int Version,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
