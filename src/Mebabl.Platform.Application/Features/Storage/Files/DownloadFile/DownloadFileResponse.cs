namespace Mebabl.Platform.Application.Features.Storage.Files.DownloadFile;

public sealed record DownloadFileResponse(
    Stream Content,
    string FileName,
    string ContentType);