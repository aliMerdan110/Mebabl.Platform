using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.CreateConversation;

public sealed record CreateConversationCommand(
    string? Title,
    bool IsGroup
) : IRequest<CreateConversationResponse>;