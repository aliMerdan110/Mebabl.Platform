using MediatR;
using Mebabl.Platform.Application.Features.SdkContent.DTOs;

namespace Mebabl.Platform.Application.Features.SdkContent.GetPosts;

public sealed record GetPostsQuery(
    int Page = 1,
    int PageSize = 20)
    : IRequest<IReadOnlyList<PostDto>>;