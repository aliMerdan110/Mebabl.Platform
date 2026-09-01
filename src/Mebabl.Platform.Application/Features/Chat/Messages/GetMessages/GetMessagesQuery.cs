using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessages;

public sealed record GetMessagesQuery(
    Guid ConversationId,
    int Offset = 0,
    int Limit = 50
) : IRequest<IReadOnlyList<GetMessagesResponse>>;