namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessages;

public sealed record GetMessagesResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    string? MessageType,
    bool IsEdited,
    DateTime? EditedAt,
    DateTime CreatedAt
);