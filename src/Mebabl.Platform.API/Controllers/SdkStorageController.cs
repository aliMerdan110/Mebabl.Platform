
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.SdkStorage.Delete;
using Mebabl.Platform.Application.Features.SdkStorage.Download;
using Mebabl.Platform.Application.Features.SdkStorage.DTOs;
using Mebabl.Platform.Application.Features.SdkStorage.List;
using Mebabl.Platform.Application.Features.SdkStorage.Metadata;
using Mebabl.Platform.Application.Features.SdkStorage.Upload;
using Mebabl.Platform.Application.Features.SdkStorage.Url;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/storage")]
[Authorize(Policy = "Application")]
public sealed class SdkStorageController : ControllerBase
{
private readonly ISender _sender;


public SdkStorageController(ISender sender)
{
    _sender = sender;
}

[HttpPost("upload")]
[Consumes("multipart/form-data")]
public async Task<ActionResult<StorageFileDto>> Upload(
    IFormFile file,
    [FromForm] string path,
    [FromForm] bool isPublic = false,
    CancellationToken cancellationToken = default)
{
    if (file is null || file.Length == 0)
        return BadRequest("File is empty.");

    await using var stream = file.OpenReadStream();

    var command = new UploadFileCommand(
        stream,
        file.FileName,
        file.ContentType,
        file.Length,
        path,
        isPublic);

    return Ok(
        await _sender.Send(
            command,
            cancellationToken));
}

[HttpGet("files")]
public async Task<IActionResult> List(
    [FromQuery] string? path,
    CancellationToken cancellationToken)
{
    return Ok(
        await _sender.Send(
            new ListFilesQuery(path),
            cancellationToken));
}

[HttpGet("files/{fileId:guid}")]
public async Task<IActionResult> Metadata(
    Guid fileId,
    CancellationToken cancellationToken)
{
    return Ok(
        await _sender.Send(
            new GetFileMetadataQuery(fileId),
            cancellationToken));
}

[HttpGet("files/{fileId:guid}/url")]
public async Task<ActionResult<StorageDownloadUrlDto>> Url(
    Guid fileId,
    CancellationToken cancellationToken)
{
    return Ok(
        await _sender.Send(
            new GetFileDownloadUrlQuery(fileId),
            cancellationToken));
}

[HttpGet("files/{fileId:guid}/download")]
public async Task<IActionResult> Download(
    Guid fileId,
    CancellationToken cancellationToken)
{
    var result = await _sender.Send(
        new DownloadFileQuery(fileId),
        cancellationToken);

    return File(
        result.Stream,
        result.ContentType,
        result.FileName);
}

[HttpDelete("files/{fileId:guid}")]
public async Task<IActionResult> Delete(
    Guid fileId,
    CancellationToken cancellationToken)
{
    await _sender.Send(
        new DeleteFileCommand(fileId),
        cancellationToken);

    return NoContent();
}

}
