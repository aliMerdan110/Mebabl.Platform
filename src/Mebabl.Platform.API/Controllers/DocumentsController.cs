using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;
using Mebabl.Platform.Application.Features.Database.Documents.GetDocument;
using Mebabl.Platform.Application.Features.Database.Documents.ListDocuments;
using Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;
using Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/data/collections/{collectionId:guid}/documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid collectionId,
        CreateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateDocumentCommand(
                collectionId,
                request.Key,
                request.Data),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ListDocumentsQuery(collectionId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{documentId:guid}")]
    public async Task<IActionResult> Get(
        Guid collectionId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetDocumentQuery(documentId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{documentId:guid}")]
public async Task<IActionResult> Update(
    Guid collectionId,
    Guid documentId,
    UpdateDocumentRequestDto request,
    CancellationToken cancellationToken)
{
    await _mediator.Send(
        new UpdateDocumentCommand(
            collectionId,
            documentId,
            request.Key,
            request.Data,
            request.ExpectedVersion),
        cancellationToken);

    return NoContent();
}

    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> Delete(
        Guid collectionId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteDocumentCommand(
                collectionId,
                documentId),
            cancellationToken);

        return NoContent();
    }

    public sealed record CreateDocumentRequest(
        string Key,
        JsonDocument Data);

    public sealed record UpdateDocumentRequestDto(
    string Key,
    JsonDocument Data,
    int ExpectedVersion);
}