using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.DeleteBucket;

public sealed class DeleteBucketCommandHandler
    : IRequestHandler<DeleteBucketCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public DeleteBucketCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        DeleteBucketCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var bucket = await _context.Buckets
            .Include(x => x.Files)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (bucket is null)
            throw new Exception("Bucket not found.");

        if (bucket.Files.Any(x => !x.IsDeleted))
            throw new Exception("Bucket is not empty.");

        bucket.IsDeleted = true;
        bucket.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}