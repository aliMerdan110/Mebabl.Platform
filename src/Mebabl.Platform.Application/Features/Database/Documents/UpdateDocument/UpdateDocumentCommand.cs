using System.Text.Json;
using MediatR;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

public sealed record UpdateDocumentCommand(
    Guid DocumentId,
    string Key,
    JsonDocument Data)
    : IRequest<DocumentResponse>;