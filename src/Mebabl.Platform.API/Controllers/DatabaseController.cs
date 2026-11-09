using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;
using Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;
using Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;
using Mebabl.Platform.Application.Features.Database.Documents.GetDocument;
using Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;
using Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

namespace Mebabl.Platform.API.Controllers;

[Authorize(Policy = "Application")]
[Route("api/database")]
public sealed class DatabaseController : BaseApiController
{
    [HttpPost("collections")]
    public async Task<IActionResult> CreateCollection(
        CreateCollectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("collections/{collectionId:guid}/documents")]
    public async Task<IActionResult> CreateDocument(
        Guid collectionId,
        [FromBody] JsonElement body,
        CancellationToken cancellationToken)
    {
        using var data = JsonDocument.Parse(body.GetRawText());

        var result = await Sender.Send(
            new CreateDocumentCommand(
                collectionId,
                Guid.NewGuid().ToString("N"),
                data),
            cancellationToken);

        return Ok(new
        {
            id = result
        });
    }

    /// <summary>
    /// Executes document create, update, and delete operations atomically
    /// within one project-owned collection.
    /// </summary>
    [HttpPost("collections/{collectionId:guid}/documents/batch")]
    public async Task<IActionResult> BatchWriteDocuments(
        Guid collectionId,
        [FromBody] BatchWriteDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Operations is null)
        {
            throw new ArgumentException(
                "Batch operations are required.",
                nameof(request.Operations));
        }

        if (request.Operations.Count is < 1 or > BatchWriteDocumentsCommand.MaximumOperations)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Operations),
                $"A batch must contain between 1 and {BatchWriteDocumentsCommand.MaximumOperations} operations.");
        }

        var operations = request.Operations
            .Select(operation =>
            {
                if (operation is null)
                {
                    throw new ArgumentException(
                        "A batch operation cannot be null.",
                        nameof(request.Operations));
                }

                return new BatchWriteDocumentOperation(
                    ParseBatchOperationType(operation.Type),
                    operation.Key,
                    operation.Data,
                    operation.DocumentId,
                    operation.ExpectedVersion);
            })
            .ToArray();

        var result = await Sender.Send(
            new BatchWriteDocumentsCommand(collectionId, operations),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("documents/{id:guid}")]
    public async Task<IActionResult> GetDocument(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetDocumentQuery(id),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("collections/{collectionId:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> Update(
        Guid collectionId,
        Guid documentId,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var data = JsonDocument.Parse(request.Data.GetRawText());

        await Sender.Send(
            new UpdateDocumentCommand(
                collectionId,
                documentId,
                request.Key,
                data,
                request.ExpectedVersion),
            cancellationToken);

        return NoContent();
    }

    public sealed record UpdateDocumentRequest(
        string Key,
        JsonElement Data,
        int ExpectedVersion);

    /// <summary>
    /// Deletes a document only when its current version matches the caller's
    /// expected version, preventing a stale client from deleting newer data.
    /// </summary>
    [HttpDelete("collections/{collectionId:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid collectionId,
        Guid documentId,
        [FromQuery(Name = "expectedVersion")] int expectedVersion,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new DeleteDocumentCommand(
                collectionId,
                documentId,
                expectedVersion),
            cancellationToken);

        return NoContent();
    }

    public sealed record BatchWriteDocumentsRequest(
        IReadOnlyList<BatchWriteDocumentOperationRequest> Operations);

    /// <summary>
    /// Uses string operation names so API clients do not depend on numeric
    /// enum values or global JSON enum-converter configuration.
    /// </summary>
    public sealed record BatchWriteDocumentOperationRequest(
        string? Type,
        string? Key = null,
        JsonElement? Data = null,
        Guid? DocumentId = null,
        int? ExpectedVersion = null);

    private static BatchWriteOperationType ParseBatchOperationType(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "create" => BatchWriteOperationType.Create,
            "update" => BatchWriteOperationType.Update,
            "delete" => BatchWriteOperationType.Delete,
            _ => throw new ArgumentException(
                "Batch operation type must be 'create', 'update', or 'delete'.",
                nameof(value))
        };
    }
}
