using MediatR;

namespace Mebabl.Platform.Application.Features.SdkStorage.Url;

public sealed record GetFileUrlQuery(
    Guid ApplicationId,
    Guid FileId
) : IRequest<string>;