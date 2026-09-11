using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Query;

public sealed class QueryDocumentsCommandValidator
    : AbstractValidator<QueryDocumentsCommand>
{
    public QueryDocumentsCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();

        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 200);
    }
}