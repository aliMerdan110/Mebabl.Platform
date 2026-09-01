using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.GetConversations;

public sealed record GetConversationsQuery(
    int Offset = 0,
    int Limit = 50
) : IRequest<IReadOnlyList<GetConversationsResponse>>;