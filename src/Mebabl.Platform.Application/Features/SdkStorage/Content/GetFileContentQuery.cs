using MediatR;

namespace Mebabl.Platform.Application.Features.SdkStorage.Content;

public sealed record GetFileContentQuery(
    Guid ApplicationId,
    Guid UserId,
    Guid FileId
) : IRequest<GetFileContentResult>;

public sealed record GetFileContentResult(
    Stream Content,
    string ContentType,
    string FileName,
    long Size);