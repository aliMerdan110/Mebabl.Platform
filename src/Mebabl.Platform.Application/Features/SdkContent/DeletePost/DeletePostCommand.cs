using MediatR;

namespace Mebabl.Platform.Application.Features.SdkContent.DeletePost;

public sealed record DeletePostCommand(Guid PostId)
    : IRequest;