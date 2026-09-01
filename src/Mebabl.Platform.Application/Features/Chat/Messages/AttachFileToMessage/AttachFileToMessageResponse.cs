namespace Mebabl.Platform.Application.Features.Chat.Messages.AttachFileToMessage;

public sealed record AttachFileToMessageResponse(
    Guid Id,
    Guid MessageId,
    Guid StoredFileId,
    string? Caption,
    DateTime CreatedAt
);