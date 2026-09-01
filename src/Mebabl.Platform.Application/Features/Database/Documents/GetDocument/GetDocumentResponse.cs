using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.Documents.GetDocument;

public sealed record GetDocumentResponse(
    Guid Id,
    string Key,
    JsonDocument Data,
    int Version,
    DateTime CreatedAt,
    DateTime? UpdatedAt);