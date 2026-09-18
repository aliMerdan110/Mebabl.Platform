namespace Mebabl.Platform.Application.Features.SdkContent.DTOs;

public sealed record PostAttachmentDto(
    Guid Id,
    Guid StorageFileId,
    string Type,
    int Order,
    string Name,
    string Path,
    string? ContentType,
    long Size,
    bool IsPublic,
    DateTime CreatedAt,
    string Url);