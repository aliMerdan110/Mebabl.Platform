using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetReactions;

public sealed record GetReactionsQuery(
    Guid MessageId
) : IRequest<IReadOnlyList<GetReactionsResponse>>;