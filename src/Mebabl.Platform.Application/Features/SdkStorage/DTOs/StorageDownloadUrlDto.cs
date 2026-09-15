namespace Mebabl.Platform.Application.Features.SdkStorage.DTOs;

public sealed record StorageDownloadUrlDto(
string Url,
DateTime? ExpiresAt
);