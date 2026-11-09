using MediatR;

namespace Mebabl.Platform.Application.Features.SdkStorage.Url;

public sealed record GetFileUrlQuery(
    Guid ApplicationId,
    Guid UserId,
    Guid FileId
) : IRequest<string>;