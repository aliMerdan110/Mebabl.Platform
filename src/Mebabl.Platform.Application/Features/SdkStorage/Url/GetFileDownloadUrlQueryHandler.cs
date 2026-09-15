
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;

namespace Mebabl.Platform.Application.Features.SdkStorage.Url;

public sealed class GetFileDownloadUrlQueryHandler
: IRequestHandler<GetFileDownloadUrlQuery, StorageDownloadUrlDto>
{
private readonly IApplicationDbContext _db;
private readonly ICurrentUser _currentUser;


public GetFileDownloadUrlQueryHandler(
    IApplicationDbContext db,
    ICurrentUser currentUser)
{
    _db = db;
    _currentUser = currentUser;
}

public async Task<StorageDownloadUrlDto> Handle(
    GetFileDownloadUrlQuery request,
    CancellationToken cancellationToken)
{
    var file = await _db.StoredFiles
        .AsNoTracking()
        .FirstOrDefaultAsync(
            x =>
                x.Id == request.FileId &&
                x.ApplicationId == _currentUser.ApplicationId &&
                !x.IsDeleted,
            cancellationToken);

    if (file is null)
        throw new KeyNotFoundException(
            "Storage file not found.");

    var url =
        $"/storage/files/{file.Id}";

    return new StorageDownloadUrlDto(
        url,
        null);
}


}
