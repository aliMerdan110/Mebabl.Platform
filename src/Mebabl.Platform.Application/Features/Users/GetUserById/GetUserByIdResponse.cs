namespace Mebabl.Platform.Application.Features.Users.GetUserById;

public sealed record GetUserByIdResponse(
    Guid Id,
    string Email,
    string Username,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);