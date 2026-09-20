using MediatR;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.PostStats.GetPostStats;

public sealed record GetPostStatsQuery(
    IReadOnlyList<Guid> PostIds)
    : IRequest<IReadOnlyList<PostSocialStatsDto>>;