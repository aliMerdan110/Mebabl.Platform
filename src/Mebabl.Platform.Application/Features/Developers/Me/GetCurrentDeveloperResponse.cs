namespace Mebabl.Platform.Application.Features.Developers.Me;

public sealed record GetCurrentDeveloperResponse(
    Guid Id,
    string DisplayName,
    string Email,
    bool IsActive);