
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Storage;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("storage/files")]
public sealed class StorageController : ControllerBase
{
private readonly IApplicationDbContext _db;
private readonly IStorageProvider _storage;


public StorageController(
    IApplicationDbContext db,
    IStorageProvider storage)
{
    _db = db;
    _storage = storage;
}

[AllowAnonymous]
[HttpGet("{fileId:guid}")]
public async Task<IActionResult> Get(
    Guid fileId,
    CancellationToken cancellationToken)
{
    var file = await _db.StoredFiles
        .AsNoTracking()
        .FirstOrDefaultAsync(
            x =>
                x.Id == fileId &&
                !x.IsDeleted,
            cancellationToken);

    if (file is null)
        return NotFound();

    if (!file.IsPublic)
        return Unauthorized();

    if (!await _storage.ExistsAsync(
            file.StorageKey,
            cancellationToken))
    {
        return NotFound();
    }

    var stream = await _storage.OpenReadAsync(
        file.StorageKey,
        cancellationToken);

    if (!string.IsNullOrWhiteSpace(file.CacheControl))
    {
        Response.Headers.CacheControl = file.CacheControl;
    }

    if (!string.IsNullOrWhiteSpace(
            file.ContentDisposition))
    {
        Response.Headers.ContentDisposition =
            file.ContentDisposition;
    }

    return File(
        stream,
        file.ContentType,
        enableRangeProcessing: true);
}


}
