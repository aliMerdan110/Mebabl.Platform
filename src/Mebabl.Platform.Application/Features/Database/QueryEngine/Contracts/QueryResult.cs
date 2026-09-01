using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

public sealed record QueryResult(
    Guid Id,
    string Key,
    JsonDocument Data,
    int Version,
    DateTime CreatedAt,
    DateTime? UpdatedAt);