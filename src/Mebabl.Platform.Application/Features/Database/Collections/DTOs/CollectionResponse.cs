namespace Mebabl.Platform.Application.Features.Database.Collections.DTOs;

/// <summary>
/// Collection metadata exposed within a project database.
/// </summary>
public sealed record CollectionResponse(
    Guid Id,
    Guid ProjectId,
    string Name,
    string Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
