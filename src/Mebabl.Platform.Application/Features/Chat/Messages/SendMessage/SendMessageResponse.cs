namespace Mebabl.Platform.Application.Features.Chat.Messages.SendMessage;

public sealed record SendMessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    string? MessageType,
    DateTime CreatedAt
);