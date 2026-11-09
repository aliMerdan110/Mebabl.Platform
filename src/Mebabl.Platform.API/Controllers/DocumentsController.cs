using System.Globalization;
using System.Text.Json;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;
using Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;
using Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;
using Mebabl.Platform.Application.Features.Database.Documents.GetDocument;
using Mebabl.Platform.Application.Features.Database.Documents.ListDocuments;
using Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

namespace Mebabl.Platform.API.Controllers;

/// <summary>
/// User-facing document operations. Project scope is resolved exclusively
/// from the validated user token by the application layer.
/// </summary>
[ApiController]
[Route("api/sdk/data/collections/{collectionId:guid}/documents")]
[Authorize(Policy = "ApplicationUser")]
[EnableCors("MebablSdk")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator
            ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DocumentResponse>> Create(
        [FromRoute] Guid collectionId,
        [FromBody] CreateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _mediator.Send(
            new CreateDocumentCommand(
                collectionId,
                request.Key,
                request.Data),
            cancellationToken);

        SetDocumentEtag(result.Version);

        return CreatedAtAction(
            nameof(Get),
            new { collectionId, documentId = result.Id },
            result);
    }

    /// <summary>
    /// Executes create, update, and delete operations atomically in one
    /// project-owned collection. The response is returned only after the
    /// application handler commits the complete transaction.
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BatchWriteDocumentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BatchWriteDocumentsResponse>> BatchWrite(
        [FromRoute] Guid collectionId,
        [FromBody] BatchWriteDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (collectionId == Guid.Empty)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid collection ID",
                Detail = "A valid collection ID is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (request.Operations is null ||
            request.Operations.Count is < 1 or > BatchWriteDocumentsCommand.MaximumOperations)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid batch operation count",
                Detail = $"A batch must contain between 1 and {BatchWriteDocumentsCommand.MaximumOperations} operations.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var operations = new List<BatchWriteDocumentOperation>(request.Operations.Count);

        for (var index = 0; index < request.Operations.Count; index++)
        {
            var operationRequest = request.Operations[index];
            if (operationRequest is null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid batch operation",
                    Detail = $"Batch operation at index {index} cannot be null.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (!TryParseBatchOperationType(operationRequest.Type, out var operationType))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid batch operation type",
                    Detail = $"Operation type at index {index} must be 'create', 'update', or 'delete'.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            operations.Add(new BatchWriteDocumentOperation(
                operationType,
                operationRequest.Key,
                operationRequest.Data,
                operationRequest.DocumentId,
                operationRequest.ExpectedVersion));
        }

        var result = await _mediator.Send(
            new BatchWriteDocumentsCommand(collectionId, operations),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DocumentResponse>>> List(
        [FromRoute] Guid collectionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ListDocumentsQuery(collectionId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{documentId:guid}")]
    [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentResponse>> Get(
        [FromRoute] Guid collectionId,
        [FromRoute] Guid documentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetDocumentQuery(documentId),
            cancellationToken);

        SetDocumentEtag(result.Version);

        return Ok(result);
    }

    [HttpPut("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid collectionId,
        [FromRoute] Guid documentId,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var preconditionError = ResolveExpectedVersion(
            ifMatch,
            out var expectedVersion);

        if (preconditionError is not null)
        {
            return preconditionError;
        }

        if (expectedVersion == int.MaxValue)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Document version exhausted",
                Detail = "The document cannot be updated because its version has reached the supported maximum.",
                Status = StatusCodes.Status409Conflict
            });
        }

        await _mediator.Send(
            new UpdateDocumentCommand(
                collectionId,
                documentId,
                request.Key,
                request.Data,
                expectedVersion),
            cancellationToken);

        SetDocumentEtag(expectedVersion + 1);

        return NoContent();
    }

    [HttpDelete("{documentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid collectionId,
        [FromRoute] Guid documentId,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        CancellationToken cancellationToken)
    {
        var preconditionError = ResolveExpectedVersion(
            ifMatch,
            out var expectedVersion);

        if (preconditionError is not null)
        {
            return preconditionError;
        }

        if (expectedVersion == int.MaxValue)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Document version exhausted",
                Detail = "The document cannot be deleted because its version has reached the supported maximum.",
                Status = StatusCodes.Status409Conflict
            });
        }

        await _mediator.Send(
            new DeleteDocumentCommand(
                collectionId,
                documentId,
                expectedVersion),
            cancellationToken);

        SetDocumentEtag(expectedVersion + 1);

        return NoContent();
    }

    private IActionResult? ResolveExpectedVersion(
        string? ifMatch,
        out int expectedVersion)
    {
        expectedVersion = 0;

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return StatusCode(
                StatusCodes.Status428PreconditionRequired,
                new ProblemDetails
                {
                    Title = "Precondition required",
                    Detail = "Send the document ETag in the If-Match header. Retrieve the document first to obtain its ETag.",
                    Status = StatusCodes.Status428PreconditionRequired
                });
        }

        var value = ifMatch.Trim();

        if (value.Length < 3 ||
            value[0] != '"' ||
            value[^1] != '"' ||
            value.Contains(',') ||
            value.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid If-Match header",
                Detail = "If-Match must contain one strong document ETag, for example \"4\".",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var versionText = value[1..^1];

        if (!int.TryParse(
                versionText,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out expectedVersion) ||
            expectedVersion < 1)
        {
            expectedVersion = 0;

            return BadRequest(new ProblemDetails
            {
                Title = "Invalid document ETag",
                Detail = "The If-Match header does not contain a valid positive document version.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return null;
    }

    private void SetDocumentEtag(int version)
    {
        Response.Headers["ETag"] = $"\"{version.ToString(CultureInfo.InvariantCulture)}\"";
    }

    private static bool TryParseBatchOperationType(
        string? value,
        out BatchWriteOperationType operationType)
    {
        operationType = value?.Trim().ToLowerInvariant() switch
        {
            "create" => BatchWriteOperationType.Create,
            "update" => BatchWriteOperationType.Update,
            "delete" => BatchWriteOperationType.Delete,
            _ => (BatchWriteOperationType)0
        };

        return Enum.IsDefined(operationType);
    }

    public sealed record CreateDocumentRequest(
        string Key,
        JsonDocument Data);

    /// <summary>
    /// ExpectedVersion is supplied through HTTP If-Match for single-document
    /// writes. Batch operations carry their own ExpectedVersion values.
    /// </summary>
    public sealed record UpdateDocumentRequest(
        string Key,
        JsonDocument Data);

    public sealed record BatchWriteDocumentsRequest(
        IReadOnlyList<BatchWriteDocumentOperationRequest>? Operations);

    public sealed record BatchWriteDocumentOperationRequest(
        string? Type,
        string? Key = null,
        JsonElement? Data = null,
        Guid? DocumentId = null,
        int? ExpectedVersion = null);
}
