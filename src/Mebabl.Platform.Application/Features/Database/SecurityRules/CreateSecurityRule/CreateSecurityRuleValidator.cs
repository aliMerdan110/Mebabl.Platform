using System.Text;
using System.Text.Json;
using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;

public sealed class CreateSecurityRuleValidator
    : AbstractValidator<CreateSecurityRuleCommand>
{
    private const int MaximumDefinitionBytes = 64 * 1024;
    private const int MaximumStatements = 100;

    public CreateSecurityRuleValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty()
            .WithMessage("Collection ID is required.");

        RuleFor(x => x.Definition)
            .Must(IsValidDefinitionEnvelope)
            .WithMessage(
                "Security policy definition must be a JSON object with version 1 and a statements array containing no more than 100 object entries.");
    }

    private static bool IsValidDefinitionEnvelope(JsonElement definition)
    {
        if (definition.ValueKind != JsonValueKind.Object)
            return false;

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
            return false;

        if (!definition.TryGetProperty("version", out var version) ||
            version.ValueKind != JsonValueKind.Number ||
            !version.TryGetInt32(out var versionNumber) ||
            versionNumber != 1)
        {
            return false;
        }

        if (!definition.TryGetProperty("statements", out var statements) ||
            statements.ValueKind != JsonValueKind.Array ||
            statements.GetArrayLength() > MaximumStatements)
        {
            return false;
        }

        return statements.EnumerateArray()
            .All(statement => statement.ValueKind == JsonValueKind.Object);
    }
}
