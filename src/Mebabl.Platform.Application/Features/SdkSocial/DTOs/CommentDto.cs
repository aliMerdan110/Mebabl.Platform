namespace Mebabl.Platform.Application.Features.SdkSocial.DTOs;

public sealed record CommentDto(
    Guid Id,
    Guid PostId,
    Guid OwnerId,
    Guid? ParentCommentId,
    string Text,
    DateTime CreatedAt,
    DateTime? UpdatedAt);