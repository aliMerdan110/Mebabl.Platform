
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;
using Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;
using Mebabl.Platform.Application.Features.Database.Documents.GetDocument;
using Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;
using Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

namespace Mebabl.Platform.API.Controllers;

[Authorize]
[Route("api/database")]
public sealed class DatabaseController : BaseApiController
{
    [HttpPost("collections")]
    public async Task<IActionResult> CreateCollection(
        CreateCollectionCommand command)
    {
        var result = await Sender.Send(command);

        return Ok(result);
    }


   [HttpPost("collections/{collectionId:guid}/documents")]
public async Task<IActionResult> CreateDocument(
    Guid collectionId,
    JsonElement body,
    CancellationToken cancellationToken)
{
    var result = await Sender.Send(
        new CreateDocumentCommand(
            collectionId,
            Guid.NewGuid().ToString("N"),
            JsonDocument.Parse(body.GetRawText())),
        cancellationToken);

    return Ok(new
    {
        id = result
    });
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

[HttpPut("documents/{id:guid}")]
public async Task<IActionResult> UpdateDocument(
    Guid id,
    JsonElement body,
    CancellationToken cancellationToken)
{
    await Sender.Send(
        new UpdateDocumentCommand(
            id,
            JsonDocument.Parse(body.GetRawText())),
        cancellationToken);

    return NoContent();
}

[HttpDelete("documents/{id:guid}")]
public async Task<IActionResult> DeleteDocument(
    Guid id,
    CancellationToken cancellationToken)
{
    await Sender.Send(
        new DeleteDocumentCommand(id),
        cancellationToken);

    return NoContent();
}


}