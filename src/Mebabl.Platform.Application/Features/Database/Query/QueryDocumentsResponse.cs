using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.Query;

/// <summary>
/// One document returned by a database query.
/// Version is the optimistic-concurrency value clients must retain
/// when they later update or delete the document.
/// </summary>
public sealed record QueryDocumentsResponse(
    Guid Id,
    string Key,
    JsonDocument Data,
    int Version,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
