namespace Mebabl.Platform.Application.Features.Applications.GetMyApplications;

public sealed record ApplicationListItem(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string? Domain,
    bool IsActive,
    DateTime CreatedAt);