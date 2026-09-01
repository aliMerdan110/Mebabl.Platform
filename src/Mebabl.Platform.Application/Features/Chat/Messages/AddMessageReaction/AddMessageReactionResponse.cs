namespace Mebabl.Platform.Application.Features.Chat.Messages.AddMessageReaction;

public sealed record AddMessageReactionResponse(
    Guid Id,
    Guid MessageId,
    Guid UserId,
    string Reaction,
    DateTime CreatedAt
);