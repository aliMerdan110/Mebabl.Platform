namespace Mebabl.Platform.Application.Features.Chat.Messages.EditMessage;

public sealed record EditMessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    string? MessageType,
    bool IsEdited,
    DateTime? EditedAt
);