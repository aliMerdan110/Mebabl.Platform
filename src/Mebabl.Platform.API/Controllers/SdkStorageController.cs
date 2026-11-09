using System.Security.Claims;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.SdkStorage.Content;
using Mebabl.Platform.Application.Features.SdkStorage.Delete;
using Mebabl.Platform.Application.Features.SdkStorage.Upload;
using Mebabl.Platform.Application.Features.SdkStorage.Url;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/storage")]
[Authorize(Policy = "ApplicationUser")]
[EnableCors("MebablSdk")]
public sealed class SdkStorageController : ControllerBase
{
    private const long MaximumUploadBytes = 50L * 1024 * 1024;
    private const int MaximumPathLength = 1024;
    private const int MaximumFileNameLength = 255;
    private const int MaximumContentTypeLength = 255;

    private readonly ISender _sender;

    public SdkStorageController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumUploadBytes)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = MaximumUploadBytes)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> Upload(
        [FromForm(Name = "file")] IFormFile? file,
        [FromForm] string? path,
        [FromForm] bool isPublic = false,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserContext(out var applicationId, out var userId))
            return Unauthorized();

        if (file is null || file.Length <= 0)
        {
            return BadRequest(new
            {
                error = "A non-empty file is required."
            });
        }

        if (file.Length > MaximumUploadBytes)
        {
            return StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                new { error = "The maximum allowed file size is 50 MB." });
        }

        if (!TryNormalizePath(path, out var normalizedPath))
        {
            return BadRequest(new
            {
                error = "The storage path is invalid."
            });
        }

        var originalFileName = (file.FileName ?? string.Empty)
            .Replace('\\', '/');

        var fileName = Path.GetFileName(originalFileName);

        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Length > MaximumFileNameLength ||
            fileName.Any(char.IsControl))
        {
            return BadRequest(new
            {
                error = "The file name is invalid."
            });
        }

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType.Trim();

        if (contentType.Length > MaximumContentTypeLength ||
            contentType.Any(char.IsControl))
        {
            return BadRequest(new
            {
                error = "The content type is invalid."
            });
        }

        if (isPublic &&
            !User.HasClaim("permission", "storage.public.write"))
        {
            return Forbid();
        }

        await using var stream = file.OpenReadStream();

        var result = await _sender.Send(
            new UploadFileCommand(
                applicationId,
                userId,
                stream,
                fileName,
                contentType,
                normalizedPath,
                isPublic),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{fileId:guid}/url")]
    public async Task<IActionResult> GetUrl(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        if (fileId == Guid.Empty)
            return BadRequest(new { error = "The file ID is invalid." });

        if (!TryGetUserContext(out var applicationId, out var userId))
            return Unauthorized();

        var result = await _sender.Send(
            new GetFileUrlQuery(
                applicationId,
                userId,
                fileId),
            cancellationToken);

        return Ok(new { url = result });
    }

    [HttpGet("{fileId:guid}/content")]
    public async Task<IActionResult> Content(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        if (fileId == Guid.Empty)
            return BadRequest(new { error = "The file ID is invalid." });

        if (!TryGetUserContext(out var applicationId, out var userId))
            return Unauthorized();

        var result = await _sender.Send(
            new GetFileContentQuery(
                applicationId,
                userId,
                fileId),
            cancellationToken);

        var fileName = Path.GetFileName(
            (result.FileName ?? string.Empty).Replace('\\', '/'));

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "download";

        var contentType = string.IsNullOrWhiteSpace(result.ContentType)
            ? "application/octet-stream"
            : result.ContentType;

        return File(
            result.Content,
            contentType,
            fileName,
            enableRangeProcessing: true);
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> Delete(
        Guid fileId,
        CancellationToken cancellationToken)
    {
        if (fileId == Guid.Empty)
            return BadRequest(new { error = "The file ID is invalid." });

        if (!TryGetUserContext(out var applicationId, out var userId))
            return Unauthorized();

        await _sender.Send(
            new DeleteFileCommand(
                applicationId,
                userId,
                fileId),
            cancellationToken);

        return NoContent();
    }

    private bool TryGetUserContext(
        out Guid applicationId,
        out Guid userId)
    {
        applicationId = Guid.Empty;
        userId = Guid.Empty;

        return
            Guid.TryParse(
                User.FindFirstValue("applicationId"),
                out applicationId) &&
            applicationId != Guid.Empty &&
            Guid.TryParse(
                User.FindFirstValue("userId"),
                out userId) &&
            userId != Guid.Empty;
    }

    private static bool TryNormalizePath(
        string? path,
        out string normalizedPath)
    {
        normalizedPath = string.Empty;

        if (string.IsNullOrWhiteSpace(path) ||
            path.Length > MaximumPathLength ||
            path.Contains('\0'))
        {
            return false;
        }

        var candidate = path.Trim().Replace('\\', '/');

        if (candidate.StartsWith('/') ||
            candidate.StartsWith("~/", StringComparison.Ordinal) ||
            (candidate.Length >= 2 &&
             char.IsLetter(candidate[0]) &&
             candidate[1] == ':'))
        {
            return false;
        }

        var segments = candidate.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        if (segments.Length == 0 ||
            segments.Any(segment =>
                segment is "." or ".." ||
                segment.Any(char.IsControl)))
        {
            return false;
        }

        normalizedPath = string.Join('/', segments);

        return normalizedPath.Length <= MaximumPathLength;
    }
}