using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.Documents.DTOs;

public sealed record DocumentResponse(
    Guid Id,
    string Key,
    JsonDocument Data,
    int Version,
    DateTime CreatedAt,
    DateTime? UpdatedAt);