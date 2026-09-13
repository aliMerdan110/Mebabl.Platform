namespace Mebabl.Platform.Application.Features.SdkStorage.DTOs;

public sealed record StorageFileDto(
    Guid Id,
    string Path,
    string Name,
    string ContentType,
    long Size,
    bool IsPublic,
    DateTime CreatedAt);