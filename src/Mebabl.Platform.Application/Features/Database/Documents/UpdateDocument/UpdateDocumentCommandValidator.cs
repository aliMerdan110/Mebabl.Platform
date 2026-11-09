using System.Text.Json;
using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

public sealed class UpdateDocumentCommandValidator
    : AbstractValidator<UpdateDocumentCommand>
{
    private const int MaximumKeyLength = 200;

    public UpdateDocumentCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();

        RuleFor(x => x.DocumentId)
            .NotEmpty();

        RuleFor(x => x.Key)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Document key is required.")
            .MaximumLength(MaximumKeyLength)
            .Must(value => value.All(character => !char.IsControl(character)))
            .WithMessage("Document key cannot contain control characters.");

        RuleFor(x => x.Data)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(data => data is not null &&
                          data.RootElement.ValueKind == JsonValueKind.Object)
            .WithMessage("Document data must be a JSON object.");

        RuleFor(x => x.ExpectedVersion)
            .GreaterThan(0)
            .WithMessage("Expected version must be a positive integer.");
    }
}
