using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AddMessageReaction;

public sealed record AddMessageReactionCommand(
    Guid MessageId,
    string Reaction
) : IRequest<AddMessageReactionResponse>;