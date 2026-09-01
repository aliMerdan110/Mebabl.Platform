using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.RemoveMessageReaction;

public sealed record RemoveMessageReactionCommand(
    Guid MessageId,
    string Reaction
) : IRequest;