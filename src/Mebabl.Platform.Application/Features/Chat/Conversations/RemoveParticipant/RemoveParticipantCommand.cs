using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.RemoveParticipant;

public sealed record RemoveParticipantCommand(
    Guid ConversationId,
    Guid UserId
) : IRequest;