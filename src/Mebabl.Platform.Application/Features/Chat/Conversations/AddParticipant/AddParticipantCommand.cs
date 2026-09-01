using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.AddParticipant;

public sealed record AddParticipantCommand(
    Guid ConversationId,
    Guid UserId,
    bool IsAdmin = false
) : IRequest;