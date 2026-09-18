using MediatR;
using Mebabl.Platform.Application.Features.SdkContent.DTOs;

namespace Mebabl.Platform.Application.Features.SdkContent.GetPost;

public sealed record GetPostQuery(Guid PostId)
    : IRequest<PostDto>;