namespace Mebabl.Platform.Application.Features.SdkSocial.DTOs;

public sealed record ShareDto(
    Guid Id,
    Guid PostId,
    Guid UserId,
    DateTime CreatedAt);