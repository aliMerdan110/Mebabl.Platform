
namespace Mebabl.Platform.Application.Features.Projects.DTOs;

public sealed record ProjectDto(
    Guid Id,
    Guid DeveloperId,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int ApplicationCount);
