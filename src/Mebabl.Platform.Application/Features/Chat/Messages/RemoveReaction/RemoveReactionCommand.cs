using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.RemoveReaction;

public sealed record RemoveReactionCommand(
    Guid MessageId
) : IRequest;