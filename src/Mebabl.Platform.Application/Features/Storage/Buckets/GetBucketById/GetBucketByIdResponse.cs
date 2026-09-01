namespace Mebabl.Platform.Application.Features.Storage.Buckets.GetBucketById;

public sealed record GetBucketByIdResponse(
    Guid Id,
    string Name,
    string Code,
    string Description,
    bool IsPublic,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);