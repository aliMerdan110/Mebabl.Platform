using MediatR;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reposts.CreateRepost;

public sealed record RepostCommand(
    Guid PostId) : IRequest<RepostDto>;