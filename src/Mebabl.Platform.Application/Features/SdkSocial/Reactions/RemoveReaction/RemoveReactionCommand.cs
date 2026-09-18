using MediatR;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reactions.RemoveReaction;

public sealed record RemoveReactionCommand(Guid PostId)
    : IRequest;