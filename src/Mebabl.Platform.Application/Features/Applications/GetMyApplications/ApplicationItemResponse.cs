namespace Mebabl.Platform.Application.Features.Applications.GetMyApplications;

public sealed record ApplicationItemResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string? Domain,
    bool IsActive);