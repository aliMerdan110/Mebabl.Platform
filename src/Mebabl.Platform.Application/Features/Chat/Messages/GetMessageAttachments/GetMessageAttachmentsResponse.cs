namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessageAttachments;

public sealed record GetMessageAttachmentsResponse(
    Guid Id,
    Guid MessageId,
    Guid StoredFileId,
    string? Caption,
    DateTime CreatedAt
);