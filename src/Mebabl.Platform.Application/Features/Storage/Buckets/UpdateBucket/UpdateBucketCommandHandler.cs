using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.UpdateBucket;

public sealed class UpdateBucketCommandHandler
    : IRequestHandler<UpdateBucketCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public UpdateBucketCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        UpdateBucketCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var bucket = await _context.Buckets
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (bucket is null)
            throw new Exception("Bucket not found.");

        bucket.Name = request.Name.Trim();
        bucket.Description = request.Description.Trim();
        bucket.IsPublic = request.IsPublic;
        bucket.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}