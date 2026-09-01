namespace Mebabl.Platform.Application.Features.Chat.Messages.GetReactions;

public sealed record GetReactionsResponse(
    Guid Id,
    Guid MessageId,
    Guid UserId,
    string Reaction,
    DateTime CreatedAt
);