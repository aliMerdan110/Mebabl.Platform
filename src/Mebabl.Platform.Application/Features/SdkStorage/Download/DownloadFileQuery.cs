using MediatR;

namespace Mebabl.Platform.Application.Features.SdkStorage.Download;

public sealed record DownloadFileQuery(
    Guid FileId) : IRequest<DownloadFileResult>;

public sealed record DownloadFileResult(
    Stream Stream,
    string ContentType,
    string FileName);