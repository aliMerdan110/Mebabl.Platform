using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.GetBucketById;

public sealed class GetBucketByIdQueryHandler
    : IRequestHandler<GetBucketByIdQuery, GetBucketByIdResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public GetBucketByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<GetBucketByIdResponse> Handle(
        GetBucketByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var bucket = await _context.Buckets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (bucket is null)
            throw new Exception("Bucket not found.");

        return new GetBucketByIdResponse(
            bucket.Id,
            bucket.Name,
            bucket.Code,
            bucket.Description,
            bucket.IsPublic,
            bucket.IsActive,
            bucket.CreatedAt,
            bucket.UpdatedAt);
    }
}