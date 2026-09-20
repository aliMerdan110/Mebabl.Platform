using MediatR;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Shares.CreateShare;

public sealed record ShareCommand(
    Guid PostId) : IRequest<ShareDto>;