using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.SdkStorage.Content;
using Mebabl.Platform.Application.Features.SdkStorage.Delete;
using Mebabl.Platform.Application.Features.SdkStorage.Upload;
using Mebabl.Platform.Application.Features.SdkStorage.Url;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize(Policy = "ApplicationUser")]
[Route("api/sdk/storage")]
public sealed class SdkStorageController : ControllerBase
{
    private readonly ISender _sender;

    public SdkStorageController(ISender sender)
    {
        _sender = sender;
    }

    // =========================================================
    // Upload
    // =========================================================

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string path,
        [FromForm] bool isPublic = false,
        CancellationToken cancellationToken = default)
    {
        if (file.Length == 0)
            return BadRequest("File is empty.");

        var applicationId = Guid.Parse(
            User.FindFirst("applicationId")!.Value);

        var userIdClaim = User.FindFirst("userId")?.Value;

        Guid? userId = Guid.TryParse(
            userIdClaim,
            out var parsedUserId)
                ? parsedUserId
                : null;

        await using var stream = file.OpenReadStream();

        var result = await _sender.Send(
            new UploadFileCommand(
                applicationId,
                userId,
                stream,
                file.FileName,
                file.ContentType,
                path,
                isPublic),
            cancellationToken);

        return Ok(result);
    }

    // =========================================================
    // URL
    // =========================================================

    [HttpGet("{fileId:guid}/url")]
    public async Task<IActionResult> GetUrl(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var applicationId = Guid.Parse(
            User.FindFirst("applicationId")!.Value);

        var result = await _sender.Send(
            new GetFileUrlQuery(
                applicationId,
                fileId),
            cancellationToken);

        return Ok(new { url = result });
    }

    // =========================================================
    // Content
    // =========================================================

    [HttpGet("{fileId:guid}/content")]
    public async Task<IActionResult> Content(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var applicationId = Guid.Parse(
            User.FindFirst("applicationId")!.Value);

        var result = await _sender.Send(
            new GetFileContentQuery(
                applicationId,
                fileId),
            cancellationToken);

        return File(
            result.Content,
            result.ContentType,
            result.FileName,
            enableRangeProcessing: true);
    }

    // =========================================================
    // Delete
    // =========================================================

    [HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> Delete(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        var applicationId = Guid.Parse(
            User.FindFirst("applicationId")!.Value);

        await _sender.Send(
            new DeleteFileCommand(
                applicationId,
                fileId),
            cancellationToken);

        return NoContent();
    }
}