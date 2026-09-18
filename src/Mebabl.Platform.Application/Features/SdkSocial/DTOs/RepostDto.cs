namespace Mebabl.Platform.Application.Features.SdkSocial.DTOs;

public sealed record RepostDto(
    Guid Id,
    Guid PostId,
    Guid UserId,
    DateTime CreatedAt);