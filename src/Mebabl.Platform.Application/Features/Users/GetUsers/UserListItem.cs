namespace Mebabl.Platform.Application.Features.Users.GetUsers;

public sealed record UserListItem(
    Guid Id,
    string Email,
    string Username,
    bool IsActive,
    DateTime CreatedAt);