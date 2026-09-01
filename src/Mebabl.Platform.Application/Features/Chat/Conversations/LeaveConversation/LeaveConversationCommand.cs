using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.LeaveConversation;

public sealed record LeaveConversationCommand(
    Guid ConversationId
) : IRequest;