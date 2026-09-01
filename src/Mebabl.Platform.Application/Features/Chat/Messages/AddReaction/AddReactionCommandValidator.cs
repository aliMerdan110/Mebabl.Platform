using FluentValidation;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AddReaction;

public sealed class AddReactionCommandValidator
    : AbstractValidator<AddReactionCommand>
{
    public AddReactionCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty();

        RuleFor(x => x.Reaction)
            .NotEmpty()
            .MaximumLength(50);
    }
}