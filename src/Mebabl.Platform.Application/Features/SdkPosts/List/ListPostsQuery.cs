using MediatR;
using Mebabl.Platform.Application.Features.SdkPosts.DTOs;

namespace Mebabl.Platform.Application.Features.SdkPosts.List;

public sealed record ListPostsQuery(
    int Skip = 0,
    int Take = 20)
    : IRequest<IReadOnlyList<PostDto>>;