using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.SendMessage;

public sealed record SendMessageCommand(
    Guid ConversationId,
    string Content,
    string? MessageType = null
) : IRequest<SendMessageResponse>;