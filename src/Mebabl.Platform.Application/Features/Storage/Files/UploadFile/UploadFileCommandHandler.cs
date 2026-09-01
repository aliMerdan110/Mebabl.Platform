using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Application.Features.Storage.Files.UploadFile;

public sealed class UploadFileCommandHandler
    : IRequestHandler<UploadFileCommand, UploadFileResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly IStorageProvider _storageProvider;

    public UploadFileCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        IStorageProvider storageProvider)
    {
        _context = context;
        _currentApplication = currentApplication;
        _storageProvider = storageProvider;
    }

    public async Task<UploadFileResponse> Handle(
        UploadFileCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var bucket = await _context.Buckets
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.BucketId &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (bucket is null)
            throw new Exception("Bucket not found.");

        var result = await _storageProvider.SaveAsync(
            bucket,
            request.FileName,
            request.ContentType,
            request.Content,
            cancellationToken);

        var storedFile = new StoredFile
        {
            BucketId = bucket.Id,
            Key = result.Key,
            FileName = request.FileName,
            ContentType = request.ContentType,
            Extension = Path.GetExtension(request.FileName),
            Size = request.Length,
            Hash = result.Hash,
            StoragePath = result.StoragePath,
            Version = 1
        };

        _context.StoredFiles.Add(storedFile);

        await _context.SaveChangesAsync(cancellationToken);

        return new UploadFileResponse(
            storedFile.Id,
            storedFile.Key,
            storedFile.FileName,
            storedFile.Size);
    }
}