using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.CreateBucket;

public sealed class CreateBucketCommandHandler
    : IRequestHandler<CreateBucketCommand, CreateBucketResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public CreateBucketCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<CreateBucketResponse> Handle(
        CreateBucketCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var exists = await _context.Buckets.AnyAsync(
            x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.Code == request.Code,
            cancellationToken);

        if (exists)
            throw new Exception("Bucket already exists.");

        var bucket = new Bucket
        {
            ApplicationId = _currentApplication.ApplicationId,
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Description = request.Description.Trim(),
            IsPublic = request.IsPublic
        };

        _context.Buckets.Add(bucket);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateBucketResponse(
            bucket.Id,
            bucket.Name,
            bucket.Code);
    }
}