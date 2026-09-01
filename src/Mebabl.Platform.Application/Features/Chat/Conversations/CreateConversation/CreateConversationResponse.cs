namespace Mebabl.Platform.Application.Features.Chat.Conversations.CreateConversation;

public sealed record CreateConversationResponse(
    Guid Id,
    string? Title,
    bool IsGroup,
    DateTime CreatedAt
);