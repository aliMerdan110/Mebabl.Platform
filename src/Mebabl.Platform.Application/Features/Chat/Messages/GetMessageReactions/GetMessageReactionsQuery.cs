using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessageReactions;

public sealed record GetMessageReactionsQuery(
    Guid MessageId
) : IRequest<IReadOnlyList<GetMessageReactionsResponse>>;