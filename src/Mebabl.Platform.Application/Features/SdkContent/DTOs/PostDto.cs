namespace Mebabl.Platform.Application.Features.SdkContent.DTOs;

public sealed record PostDto(
    Guid Id,
    Guid OwnerId,
    string? Text,
    DateTime CreatedAt,
    IReadOnlyList<PostAttachmentDto> Attachments);