namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessageReactions;

public sealed record GetMessageReactionsResponse(
    Guid Id,
    Guid MessageId,
    Guid UserId,
    string Reaction,
    DateTime CreatedAt
);