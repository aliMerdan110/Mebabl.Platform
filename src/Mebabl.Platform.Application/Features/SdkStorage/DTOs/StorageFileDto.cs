namespace Mebabl.Platform.Application.Features.SdkStorage.DTOs;

public sealed record StorageFileDto(
    Guid Id,
    string Name,
    string Path,
    string ContentType,
    long Size,
    bool IsPublic,
    DateTime CreatedAt,
    string Url,
    Guid? OwnerId
);