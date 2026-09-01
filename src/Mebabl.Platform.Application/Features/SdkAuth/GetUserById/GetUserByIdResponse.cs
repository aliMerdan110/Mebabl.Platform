namespace Mebabl.Platform.Application.Features.SdkAuth.GetUserById;

public sealed record GetUserByIdResponse(
    Guid Id,
    string Email,
    string Username,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);