namespace Mebabl.Platform.Application.Features.Chat.Conversations.GetConversations;

public sealed record GetConversationsResponse(
    Guid Id,
    string? Title,
    bool IsGroup,
    DateTime CreatedAt,
    IReadOnlyList<Guid> ParticipantIds
);