
using System.Text.Json;
using MediatR;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;

public sealed record CreateDocumentCommand(
    Guid CollectionId,
    string Key,
    JsonDocument Data
) : IRequest<DocumentResponse>;
