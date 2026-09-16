using MediatR;
using Mebabl.Platform.Application.Features.SdkPosts.DTOs;

namespace Mebabl.Platform.Application.Features.SdkPosts.Create;

public sealed record CreatePostCommand(
    string? Text,
    IReadOnlyList<Guid>? StorageFileIds)
    : IRequest<PostDto>;