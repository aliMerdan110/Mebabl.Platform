using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Storage.Buckets.CreateBucket;
using Mebabl.Platform.Application.Features.Storage.Buckets.DeleteBucket;
using Mebabl.Platform.Application.Features.Storage.Buckets.GetBucketById;
using Mebabl.Platform.Application.Features.Storage.Buckets.GetBuckets;
using Mebabl.Platform.Application.Features.Storage.Buckets.UpdateBucket;
using Mebabl.Platform.Application.Features.Storage.Files.UploadFile;

using Mebabl.Platform.Application.Features.Storage.Files.GetFileById;
using Mebabl.Platform.Application.Features.Storage.Files.DownloadFile;
using Mebabl.Platform.Application.Features.Storage.Files.DeleteFile;

namespace Mebabl.Platform.API.Controllers;

[Authorize]
[ApiController]
[Route("api/storage")]
public sealed class StorageController : BaseApiController
{
    [HttpPost("buckets")]
    public async Task<IActionResult> CreateBucket(
        CreateBucketCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpGet("buckets")]
    public async Task<IActionResult> GetBuckets(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetBucketsQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("buckets/{id:guid}")]
    public async Task<IActionResult> GetBucketById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetBucketByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("buckets/{id:guid}")]
    public async Task<IActionResult> UpdateBucket(
        Guid id,
        UpdateBucketCommand command,
        CancellationToken cancellationToken)
    {
        command = command with { Id = id };

        await Sender.Send(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("buckets/{id:guid}")]
    public async Task<IActionResult> DeleteBucket(
        Guid id,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new DeleteBucketCommand(id),
            cancellationToken);

        return NoContent();
    }
  

  [HttpPost("buckets/{bucketId:guid}/files")]
public async Task<IActionResult> UploadFile(
    Guid bucketId,
    IFormFile file,
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        new UploadFileCommand(
            bucketId,
            file.FileName,
            file.ContentType,
            file.Length,
            file.OpenReadStream()),
        cancellationToken);

    return Ok(result);
}


[HttpGet("files/{id:guid}")]
public async Task<IActionResult> GetFile(
    Guid id,
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        new GetFileByIdQuery(id),
        cancellationToken);

    return Ok(result);
}


[HttpGet("files/{id:guid}/download")]
public async Task<IActionResult> DownloadFile(
    Guid id,
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        new DownloadFileQuery(id),
        cancellationToken);

    return File(
        result.Content,
        result.ContentType,
        result.FileName);
}


[HttpDelete("files/{id:guid}")]
public async Task<IActionResult> DeleteFile(
    Guid id,
    CancellationToken cancellationToken)
{
    await Sender.Send(
        new DeleteFileCommand(id),
        cancellationToken);

    return NoContent();
}


}