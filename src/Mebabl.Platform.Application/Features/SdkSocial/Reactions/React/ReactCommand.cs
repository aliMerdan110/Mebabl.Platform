using MediatR;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reactions.React;

public sealed record ReactCommand(
    Guid PostId,
    string Type)
    : IRequest<ReactionDto>;