using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.GetBuckets;

public sealed record GetBucketsQuery
    : IRequest<IReadOnlyList<BucketListItem>>;