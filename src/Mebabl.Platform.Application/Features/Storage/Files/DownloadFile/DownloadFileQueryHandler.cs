using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.Application.Features.Storage.Files.DownloadFile;

public sealed class DownloadFileQueryHandler
    : IRequestHandler<DownloadFileQuery, DownloadFileResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly IStorageProvider _storageProvider;

    public DownloadFileQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        IStorageProvider storageProvider)
    {
        _context = context;
        _currentApplication = currentApplication;
        _storageProvider = storageProvider;
    }

    public async Task<DownloadFileResponse> Handle(
        DownloadFileQuery request,
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

        var stream = await _storageProvider.OpenReadAsync(
            file,
            cancellationToken);

        return new DownloadFileResponse(
            stream,
            file.FileName,
            file.ContentType);
    }
}