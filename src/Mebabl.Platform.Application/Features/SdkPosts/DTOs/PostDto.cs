namespace Mebabl.Platform.Application.Features.SdkPosts.DTOs;

public sealed record PostDto(
    Guid Id,
    Guid OwnerId,
    string? Text,
    DateTime CreatedAt,
    IReadOnlyList<PostAttachmentDto> Attachments);