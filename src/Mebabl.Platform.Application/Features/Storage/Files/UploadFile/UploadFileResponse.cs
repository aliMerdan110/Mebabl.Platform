namespace Mebabl.Platform.Application.Features.Storage.Files.UploadFile;

public sealed record UploadFileResponse(
    Guid Id,
    string Key,
    string FileName,
    long Size);