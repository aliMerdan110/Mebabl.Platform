using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.GetBuckets;

public sealed class GetBucketsQueryHandler
    : IRequestHandler<GetBucketsQuery, IReadOnlyList<BucketListItem>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public GetBucketsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<BucketListItem>> Handle(
        GetBucketsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await _context.Buckets
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new BucketListItem(
                x.Id,
                x.Name,
                x.Code,
                x.IsPublic,
                x.IsActive,
                x.Files.Count(f => !f.IsDeleted),
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}