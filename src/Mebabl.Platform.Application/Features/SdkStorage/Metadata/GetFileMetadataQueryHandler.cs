using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.Metadata;

public sealed class GetFileMetadataQueryHandler
    : IRequestHandler<
        GetFileMetadataQuery,
        StorageFileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public GetFileMetadataQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<StorageFileDto> Handle(
        GetFileMetadataQuery request,
        CancellationToken cancellationToken)
    {
        var applicationId =
            _currentApplication.ApplicationId;

        var file = await _db.StoredFiles
            .AsNoTracking()
            .Where(x =>
                x.Id == request.FileId &&
                x.ApplicationId == applicationId)
            .Select(x => new StorageFileDto(
                x.Id,
                x.Path,
                x.Name,
                x.ContentType,
                x.Size,
                x.IsPublic,
                x.CreatedAt,
                $"/storage/files/{x.Id}"))
            .FirstOrDefaultAsync(cancellationToken);

        return file
            ?? throw new KeyNotFoundException(
                "File was not found.");
    }
}