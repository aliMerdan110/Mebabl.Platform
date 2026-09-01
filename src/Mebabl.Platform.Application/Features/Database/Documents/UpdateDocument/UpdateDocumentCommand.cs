using System.Text.Json;
using MediatR;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

public sealed record UpdateDocumentCommand(
    Guid Id,
    JsonDocument Data
) : IRequest;