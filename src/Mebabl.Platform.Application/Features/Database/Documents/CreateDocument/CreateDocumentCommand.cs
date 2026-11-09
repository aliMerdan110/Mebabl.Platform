using System.Text.Json;
using MediatR;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;

/// <summary>
/// Creates a document in the authenticated project's collection.
/// Project, application, and creator identifiers are resolved from the
/// validated server-side authentication context, never from this command.
/// </summary>
public sealed record CreateDocumentCommand(
    Guid CollectionId,
    string Key,
    JsonDocument Data
) : IRequest<DocumentResponse>;
