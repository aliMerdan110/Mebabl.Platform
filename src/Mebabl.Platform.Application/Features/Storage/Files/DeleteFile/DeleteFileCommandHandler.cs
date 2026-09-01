using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Application.Features.Storage.Files.DeleteFile;

public sealed class DeleteFileCommandHandler
    : IRequestHandler<DeleteFileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly IStorageProvider _storageProvider;

    public DeleteFileCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        IStorageProvider storageProvider)
    {
        _context = context;
        _currentApplication = currentApplication;
        _storageProvider = storageProvider;
    }

    public async Task Handle(
        DeleteFileCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var file = await _context.StoredFiles
            .Include(x => x.Bucket)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.Bucket.ApplicationId == _currentApplication.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (file is null)
            throw new Exception("File not found.");

        await _storageProvider.DeleteAsync(
            file,
            cancellationToken);

        file.IsDeleted = true;
        file.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}