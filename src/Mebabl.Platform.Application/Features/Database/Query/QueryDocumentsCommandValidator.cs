using System.Text.Json;
using FluentValidation;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

namespace Mebabl.Platform.Application.Features.Database.Query;

public sealed class QueryDocumentsCommandValidator
    : AbstractValidator<QueryDocumentsCommand>
{
    private static readonly HashSet<string> SupportedSortFields =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "createdAt",
            "updatedAt",
            "key",
            "version"
        };

    public QueryDocumentsCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();

        RuleFor(x => x.Offset)
            .InclusiveBetween(0, QueryRequest.MaximumOffset);

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, QueryRequest.MaximumPageSize);

        RuleFor(x => x.Filters)
            .Must(filters => filters is null ||
                             filters.Count <= QueryRequest.MaximumFilters)
            .WithMessage(
                $"A query cannot contain more than {QueryRequest.MaximumFilters} filters.");

        RuleFor(x => x.Sorts)
            .Must(sorts => sorts is null ||
                           sorts.Count <= QueryRequest.MaximumSorts)
            .WithMessage(
                $"A query cannot contain more than {QueryRequest.MaximumSorts} sort fields.");

        RuleFor(x => x.Search)
            .MaximumLength(QueryRequest.MaximumSearchLength)
            .Must(search => search is null || !search.Any(char.IsControl))
            .WithMessage("Search cannot contain control characters.");

        RuleFor(x => x.PageToken)
            .MaximumLength(QueryDocumentsCommand.MaximumPageTokenLength)
            .Must(token => token is null ||
                           (!string.IsNullOrWhiteSpace(token) &&
                            token.All(character =>
                                char.IsAsciiLetterOrDigit(character) ||
                                character is '-' or '_')))
            .WithMessage("PageToken is invalid or exceeds the supported length.");

        When(x => x.PageToken is not null, () =>
        {
            RuleFor(x => x.Offset)
                .Equal(0)
                .WithMessage("Offset must be zero when PageToken is supplied.");

            RuleFor(x => x.Sorts)
                .Must(sorts => sorts is null || sorts.Count == 0)
                .WithMessage("Cursor pagination currently uses the default CreatedAt DESC, Id DESC order.");
        });

        // The current response contract returns complete documents. Reject
        // projection explicitly instead of silently returning unprojected data.
        RuleFor(x => x.Select)
            .Must(fields => fields is null || fields.Count == 0)
            .WithMessage(
                "Field projection is not supported by this query result contract.");

        When(x => x.Filters is { Count: > 0 }, () =>
        {
            RuleForEach(x => x.Filters!)
                .NotNull()
                .WithMessage("A query filter cannot be null.")
                .ChildRules(filter =>
                {
                    filter.RuleFor(x => x.Field)
                        .Must(IsValidFieldPath)
                        .WithMessage("Filter field path is invalid.");

                    filter.RuleFor(x => x.Operator)
                        .IsInEnum();

                    filter.RuleFor(x => x)
                        .Must(HasCompatibleScalarValue)
                        .WithMessage(
                            "The filter value must be a supported scalar value for the selected operator.");
                });
        });

        When(x => x.Sorts is { Count: > 0 }, () =>
        {
            RuleForEach(x => x.Sorts!)
                .NotNull()
                .WithMessage("A sort definition cannot be null.")
                .ChildRules(sort =>
                {
                    sort.RuleFor(x => x.Field)
                        .Must(field =>
                            !string.IsNullOrWhiteSpace(field) &&
                            SupportedSortFields.Contains(field.Trim()))
                        .WithMessage(
                            "Sort field must be one of: createdAt, updatedAt, key, version.");

                    sort.RuleFor(x => x.Direction)
                        .Must(direction =>
                            direction is not null &&
                            (direction.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                             direction.Equals("desc", StringComparison.OrdinalIgnoreCase)))
                        .WithMessage("Sort direction must be 'asc' or 'desc'.");
                });

            RuleFor(x => x.Sorts)
                .Must(HaveUniqueSortFields)
                .WithMessage("A sort field cannot be specified more than once.");
        });
    }

    private static bool IsValidFieldPath(string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return false;

        var normalized = field.Trim();
        if (normalized.Length > 128 || normalized.Any(char.IsControl))
            return false;

        var segments = normalized.Split('.', StringSplitOptions.None);
        if (segments.Length is < 1 or > 8)
            return false;

        return segments.All(segment =>
            segment.Length is > 0 and <= 64 &&
            (char.IsLetter(segment[0]) || segment[0] == '_') &&
            segment.All(character =>
                char.IsLetterOrDigit(character) ||
                character == '_' ||
                character == '-'));
    }

    private static bool HaveUniqueSortFields(
        IReadOnlyList<QuerySort>? sorts)
    {
        if (sorts is null)
            return true;

        var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return sorts.All(sort =>
            sort is not null &&
            !string.IsNullOrWhiteSpace(sort.Field) &&
            fields.Add(sort.Field.Trim()));
    }

    private static bool HasCompatibleScalarValue(QueryFilter filter)
    {
        if (filter is null || filter.Value.ValueKind == JsonValueKind.Undefined)
            return false;

        var value = filter.Value;
        var valueKind = value.ValueKind;
        var stringValue = valueKind == JsonValueKind.String
            ? value.GetString()
            : null;

        if (valueKind is not (
            JsonValueKind.String or
            JsonValueKind.Number or
            JsonValueKind.True or
            JsonValueKind.False or
            JsonValueKind.Null))
        {
            return false;
        }

        return filter.Operator switch
        {
            QueryOperator.Equal or QueryOperator.NotEqual => true,

            QueryOperator.GreaterThan or
            QueryOperator.GreaterThanOrEqual or
            QueryOperator.LessThan or
            QueryOperator.LessThanOrEqual =>
                valueKind == JsonValueKind.Number,

            QueryOperator.Contains or
            QueryOperator.StartsWith or
            QueryOperator.EndsWith =>
                valueKind == JsonValueKind.String &&
                !string.IsNullOrEmpty(stringValue) &&
                stringValue.Length <= 256,

            _ => false
        };
    }
}
