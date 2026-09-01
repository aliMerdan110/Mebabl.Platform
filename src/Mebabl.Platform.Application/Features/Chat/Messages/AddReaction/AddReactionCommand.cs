using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AddReaction;

public sealed record AddReactionCommand(
    Guid MessageId,
    string Reaction
) : IRequest;