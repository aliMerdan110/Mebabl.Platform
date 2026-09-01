using FluentValidation;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AttachFileToMessage;

public sealed class AttachFileToMessageCommandValidator
    : AbstractValidator<AttachFileToMessageCommand>
{
    public AttachFileToMessageCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty();

        RuleFor(x => x.StoredFileId)
            .NotEmpty();

        RuleFor(x => x.Caption)
            .MaximumLength(500)
            .When(x => x.Caption is not null);
    }
}