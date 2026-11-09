using System.Text;
using System.Text.Json;
using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.UpdateSecurityRule;

public sealed class UpdateSecurityRuleValidator
    : AbstractValidator<UpdateSecurityRuleCommand>
{
    private const int MaximumDefinitionBytes = 64 * 1024;
    private const int MaximumJsonDepth = 16;
    private const int MaximumStatements = 100;

    public UpdateSecurityRuleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Security policy ID is required.");

        RuleFor(x => x.ExpectedRevision)
            .GreaterThan(0)
            .WithMessage("Expected revision must be a positive number.");

        RuleFor(x => x.Definition)
            .Must(IsValidDefinition)
            .WithMessage(
                "Security policy definition must be a JSON object with version 1 and a statements array containing no more than 100 object entries, and must not exceed 64 KB or JSON depth 16.");
    }

    private static bool IsValidDefinition(JsonElement definition)
    {
        if (definition.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        string rawJson;
        try
        {
            rawJson = definition.GetRawText();
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        if (Encoding.UTF8.GetByteCount(rawJson) > MaximumDefinitionBytes)
        {
            return false;
        }

        JsonDocument parsed;
        try
        {
            parsed = JsonDocument.Parse(
                rawJson,
                new JsonDocumentOptions
                {
                    MaxDepth = MaximumJsonDepth,
                    CommentHandling = JsonCommentHandling.Disallow,
                    AllowTrailingCommas = false
                });
        }
        catch (JsonException)
        {
            return false;
        }

        using (parsed)
        {
            var root = parsed.RootElement;

            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("version", out var version) ||
                version.ValueKind != JsonValueKind.Number ||
                !version.TryGetInt32(out var versionNumber) ||
                versionNumber != 1)
            {
                return false;
            }

            if (!root.TryGetProperty("statements", out var statements) ||
                statements.ValueKind != JsonValueKind.Array ||
                statements.GetArrayLength() > MaximumStatements)
            {
                return false;
            }

            return statements.EnumerateArray()
                .All(statement => statement.ValueKind == JsonValueKind.Object);
        }
    }
}
