using MediatR;
using Mebabl.Platform.Application.Features.SdkContent.DTOs;

namespace Mebabl.Platform.Application.Features.SdkContent.CreatePost;

public sealed record CreatePostCommand(
    string? Text,
    IReadOnlyList<Guid>? StorageFileIds)
    : IRequest<PostDto>;