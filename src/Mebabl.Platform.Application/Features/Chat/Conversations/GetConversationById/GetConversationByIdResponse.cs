namespace Mebabl.Platform.Application.Features.Chat.Conversations.GetConversationById;

public sealed record GetConversationByIdResponse(
    Guid Id,
    string? Title,
    bool IsGroup,
    DateTime CreatedAt,
    IReadOnlyList<Guid> ParticipantIds
);