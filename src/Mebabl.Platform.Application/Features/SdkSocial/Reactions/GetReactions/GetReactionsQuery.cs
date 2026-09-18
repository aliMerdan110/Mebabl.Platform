using MediatR;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reactions.GetReactions;

public sealed record GetReactionsQuery(Guid PostId)
    : IRequest<IReadOnlyList<ReactionDto>>;