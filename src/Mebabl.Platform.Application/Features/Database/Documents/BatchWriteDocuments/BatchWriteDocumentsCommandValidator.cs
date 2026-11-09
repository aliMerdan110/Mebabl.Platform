using System.Text;
using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;

/// <summary>
/// Validates the complete shape and resource limits of an atomic document
/// batch before the handler opens a database transaction.
/// Database-dependent checks (ownership, permissions, key availability and
/// expected document versions) remain in the command handler.
/// </summary>
public sealed class BatchWriteDocumentsCommandValidator
    : AbstractValidator<BatchWriteDocumentsCommand>
{
    private const int MaximumKeyLength = 200;

    public BatchWriteDocumentsCommandValidator()
    {
        RuleFor(command => command.CollectionId)
            .NotEmpty()
            .WithMessage("Collection ID is required.");

        RuleFor(command => command)
            .Custom(ValidateOperations);
    }

    private static void ValidateOperations(
        BatchWriteDocumentsCommand command,
        ValidationContext<BatchWriteDocumentsCommand> context)
    {
        var operations = command.Operations;
        if (operations is null)
        {
            context.AddFailure("Operations", "Batch operations are required.");
            return;
        }

        if (operations.Count is < 1 or > BatchWriteDocumentsCommand.MaximumOperations)
        {
            context.AddFailure(
                "Operations",
                $"A batch must contain between 1 and {BatchWriteDocumentsCommand.MaximumOperations} operations.");
            return;
        }

        var targetedDocumentIds = new HashSet<Guid>();
        var desiredKeys = new HashSet<string>(StringComparer.Ordinal);
        long totalPayloadBytes = 0;

        for (var index = 0; index < operations.Count; index++)
        {
            var operation = operations[index];
            var prefix = $"Operations[{index}]";

            if (operation is null)
            {
                context.AddFailure(prefix, "A batch operation cannot be null.");
                continue;
            }

            if (!Enum.IsDefined(operation.Type))
            {
                context.AddFailure(
                    $"{prefix}.Type",
                    "Operation type must be Create, Update, or Delete.");
                continue;
            }

            switch (operation.Type)
            {
                case BatchWriteOperationType.Create:
                    if (operation.DocumentId.HasValue || operation.ExpectedVersion.HasValue)
                    {
                        context.AddFailure(
                            prefix,
                            "Create operations cannot specify DocumentId or ExpectedVersion.");
                    }

                    ValidateKey(operation.Key, prefix, context, desiredKeys);
                    ValidateData(
                        operation.Data,
                        index,
                        prefix,
                        context,
                        ref totalPayloadBytes);
                    break;

                case BatchWriteOperationType.Update:
                    ValidateDocumentId(
                        operation.DocumentId,
                        index,
                        prefix,
                        context,
                        targetedDocumentIds);
                    ValidateExpectedVersion(operation.ExpectedVersion, prefix, context);
                    ValidateKey(operation.Key, prefix, context, desiredKeys);
                    ValidateData(
                        operation.Data,
                        index,
                        prefix,
                        context,
                        ref totalPayloadBytes);
                    break;

                case BatchWriteOperationType.Delete:
                    ValidateDocumentId(
                        operation.DocumentId,
                        index,
                        prefix,
                        context,
                        targetedDocumentIds);
                    ValidateExpectedVersion(operation.ExpectedVersion, prefix, context);

                    if (operation.Key is not null || operation.Data.HasValue)
                    {
                        context.AddFailure(
                            prefix,
                            "Delete operations cannot specify Key or Data.");
                    }
                    break;
            }
        }
    }

    private static void ValidateDocumentId(
        Guid? documentId,
        int index,
        string prefix,
        ValidationContext<BatchWriteDocumentsCommand> context,
        HashSet<Guid> targetedDocumentIds)
    {
        if (!documentId.HasValue || documentId.Value == Guid.Empty)
        {
            context.AddFailure(
                $"{prefix}.DocumentId",
                $"A valid DocumentId is required at batch operation index {index}.");
            return;
        }

        if (!targetedDocumentIds.Add(documentId.Value))
        {
            context.AddFailure(
                $"{prefix}.DocumentId",
                "A document can be targeted only once in a batch.");
        }
    }

    private static void ValidateExpectedVersion(
        int? expectedVersion,
        string prefix,
        ValidationContext<BatchWriteDocumentsCommand> context)
    {
        if (!expectedVersion.HasValue || expectedVersion.Value < 1)
        {
            context.AddFailure(
                $"{prefix}.ExpectedVersion",
                "A positive ExpectedVersion is required for update and delete operations.");
        }
    }

    private static void ValidateKey(
        string? key,
        string prefix,
        ValidationContext<BatchWriteDocumentsCommand> context,
        HashSet<string> desiredKeys)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            context.AddFailure($"{prefix}.Key", "Document key is required.");
            return;
        }

        var normalized = key.Trim();
        if (normalized.Length > MaximumKeyLength || normalized.Any(char.IsControl))
        {
            context.AddFailure(
                $"{prefix}.Key",
                $"Document key cannot exceed {MaximumKeyLength} characters or contain control characters.");
            return;
        }

        if (!desiredKeys.Add(normalized))
        {
            context.AddFailure(
                $"{prefix}.Key",
                $"The batch contains more than one document with key '{normalized}'.");
        }
    }

    private static void ValidateData(
        System.Text.Json.JsonElement? data,
        int index,
        string prefix,
        ValidationContext<BatchWriteDocumentsCommand> context,
        ref long totalPayloadBytes)
    {
        if (!data.HasValue || data.Value.ValueKind != System.Text.Json.JsonValueKind.Object)
        {
            context.AddFailure(
                $"{prefix}.Data",
                $"Data for batch operation at index {index} must be a JSON object.");
            return;
        }

        try
        {
            totalPayloadBytes += Encoding.UTF8.GetByteCount(data.Value.GetRawText());
        }
        catch (ObjectDisposedException)
        {
            context.AddFailure(
                $"{prefix}.Data",
                "The JSON data for this operation is no longer available.");
            return;
        }

        if (totalPayloadBytes > BatchWriteDocumentsCommand.MaximumPayloadBytes)
        {
            context.AddFailure(
                "Operations",
                $"Aggregate batch data cannot exceed {BatchWriteDocumentsCommand.MaximumPayloadBytes} UTF-8 bytes.");
        }
    }
}
