namespace Mebabl.Platform.Application.Features.SdkSocial.DTOs;

public sealed record ReactionDto(
    Guid Id,
    Guid PostId,
    Guid UserId,
    string Type,
    DateTime CreatedAt);