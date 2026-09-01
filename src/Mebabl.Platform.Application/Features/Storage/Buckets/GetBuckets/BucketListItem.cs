namespace Mebabl.Platform.Application.Features.Storage.Buckets.GetBuckets;

public sealed record BucketListItem(
    Guid Id,
    string Name,
    string Code,
    bool IsPublic,
    bool IsActive,
    int FilesCount,
    DateTime CreatedAt);