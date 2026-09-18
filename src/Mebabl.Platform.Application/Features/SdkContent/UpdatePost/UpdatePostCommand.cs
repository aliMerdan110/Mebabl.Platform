using MediatR;
using Mebabl.Platform.Application.Features.SdkContent.DTOs;

namespace Mebabl.Platform.Application.Features.SdkContent.UpdatePost;

public sealed record UpdatePostCommand(
    Guid PostId,
    string? Text)
    : IRequest<PostDto>;