namespace Mebabl.Platform.Application.Common.Storage;

public sealed record StorageResult(
    string Key,
    string Hash,
    string StoragePath);