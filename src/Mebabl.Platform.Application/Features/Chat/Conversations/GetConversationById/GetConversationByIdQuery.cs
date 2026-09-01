using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.GetConversationById;

public sealed record GetConversationByIdQuery(
    Guid ConversationId
) : IRequest<GetConversationByIdResponse>;