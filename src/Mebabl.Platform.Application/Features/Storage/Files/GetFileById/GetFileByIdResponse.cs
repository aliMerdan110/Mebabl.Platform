namespace Mebabl.Platform.Application.Features.Storage.Files.GetFileById;

public sealed record GetFileByIdResponse(
    Guid Id,
    Guid BucketId,
    string Key,
    string FileName,
    string ContentType,
    string Extension,
    long Size,
    string Hash,
    int Version,
    DateTime CreatedAt);