namespace Mebabl.Platform.Application.Features.Applications.Users;

public sealed record ApplicationUserDto(
    Guid Id,
    string Email,
    string Username,
    string Providers,
    DateTime CreatedAt,
    DateTime? LastSignInAt,
    bool IsActive);