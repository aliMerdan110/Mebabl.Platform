using System.Text.Json;
using MediatR;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

public sealed record UpdateDocumentCommand(
    Guid CollectionId,
    Guid DocumentId,
    string Key,
    JsonDocument Data,
    int ExpectedVersion
) : IRequest;