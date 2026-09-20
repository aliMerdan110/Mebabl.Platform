using MediatR;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.GetComments;

public sealed record GetCommentsQuery(
    Guid PostId,
    int Page = 1,
    int PageSize = 20)
    : IRequest<IReadOnlyList<CommentDto>>;