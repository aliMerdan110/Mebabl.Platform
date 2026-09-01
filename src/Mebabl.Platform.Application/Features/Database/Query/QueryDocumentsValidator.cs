using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Query;

public sealed class QueryDocumentsValidator
    : AbstractValidator<QueryDocumentsCommand>
{
    public QueryDocumentsValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(500);

        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0);
    }
}