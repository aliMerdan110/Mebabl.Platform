using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Files.DownloadFile;

public sealed record DownloadFileQuery(
    Guid Id
) : IRequest<DownloadFileResponse>;