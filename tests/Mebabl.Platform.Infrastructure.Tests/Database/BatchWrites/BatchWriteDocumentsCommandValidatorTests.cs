using System.Text.Json;
using Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;
using Xunit;

namespace Mebabl.Platform.Infrastructure.Tests.Database.BatchWrites;

public sealed class BatchWriteDocumentsCommandValidatorTests
{
    private readonly BatchWriteDocumentsCommandValidator _validator = new();

    [Fact]
    public void ValidCreateOperation_PassesValidation()
    {
        var command = new BatchWriteDocumentsCommand(
            Guid.NewGuid(),
            new[]
            {
                new BatchWriteDocumentOperation(
                    BatchWriteOperationType.Create,
                    "customer-001",
                    JsonObject("{\"name\":\"Ali\"}"))
            });

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyCollectionId_FailsValidation()
    {
        var command = new BatchWriteDocumentsCommand(
            Guid.Empty,
            new[] { CreateOperation("one") });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "CollectionId");
    }

    [Fact]
    public void EmptyOperations_FailsValidation()
    {
        var command = new BatchWriteDocumentsCommand(Guid.NewGuid(), Array.Empty<BatchWriteDocumentOperation>());

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Operations");
    }

    [Fact]
    public void MoreThanMaximumOperations_FailsValidation()
    {
        var operations = Enumerable.Range(0, BatchWriteDocumentsCommand.MaximumOperations + 1)
            .Select(index => CreateOperation($"key-{index}"))
            .ToArray();
        var command = new BatchWriteDocumentsCommand(Guid.NewGuid(), operations);

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Operations");
    }

    [Fact]
    public void UpdateWithoutExpectedVersion_FailsValidation()
    {
        var command = new BatchWriteDocumentsCommand(
            Guid.NewGuid(),
            new[]
            {
                new BatchWriteDocumentOperation(
                    BatchWriteOperationType.Update,
                    "customer-001",
                    JsonObject("{}"),
                    Guid.NewGuid())
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith("ExpectedVersion", StringComparison.Ordinal));
    }

    [Fact]
    public void DeleteWithData_FailsValidation()
    {
        var command = new BatchWriteDocumentsCommand(
            Guid.NewGuid(),
            new[]
            {
                new BatchWriteDocumentOperation(
                    BatchWriteOperationType.Delete,
                    Data: JsonObject("{}"),
                    DocumentId: Guid.NewGuid(),
                    ExpectedVersion: 1)
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("cannot specify Key or Data", StringComparison.Ordinal));
    }

    [Fact]
    public void SameDocumentCannotBeTargetedTwice()
    {
        var documentId = Guid.NewGuid();
        var command = new BatchWriteDocumentsCommand(
            Guid.NewGuid(),
            new[]
            {
                new BatchWriteDocumentOperation(
                    BatchWriteOperationType.Update,
                    "one",
                    JsonObject("{}"),
                    documentId,
                    1),
                new BatchWriteDocumentOperation(
                    BatchWriteOperationType.Delete,
                    DocumentId: documentId,
                    ExpectedVersion: 1)
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("targeted only once", StringComparison.Ordinal));
    }

    [Fact]
    public void DuplicateDesiredKeys_FailValidation()
    {
        var command = new BatchWriteDocumentsCommand(
            Guid.NewGuid(),
            new[]
            {
                CreateOperation("same-key"),
                CreateOperation(" same-key ")
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("more than one document with key", StringComparison.Ordinal));
    }

    [Fact]
    public void AggregatePayloadOverLimit_FailsValidation()
    {
        var largeValue = new string('x', BatchWriteDocumentsCommand.MaximumPayloadBytes);
        var data = JsonSerializer.SerializeToElement(new { value = largeValue });
        var command = new BatchWriteDocumentsCommand(
            Guid.NewGuid(),
            new[]
            {
                new BatchWriteDocumentOperation(
                    BatchWriteOperationType.Create,
                    "large",
                    data)
            });

        var result = _validator.Validate(command);

        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Aggregate batch data", StringComparison.Ordinal));
    }

    private static BatchWriteDocumentOperation CreateOperation(string key) =>
        new(BatchWriteOperationType.Create, key, JsonObject("{}"));

    private static JsonElement JsonObject(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
