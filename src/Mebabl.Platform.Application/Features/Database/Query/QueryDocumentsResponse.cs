using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.Query;

public sealed record QueryDocumentsResponse(
    Guid Id,
    string Key,
    JsonDocument Data,
    int Version,
    DateTime CreatedAt,
    DateTime? UpdatedAt);