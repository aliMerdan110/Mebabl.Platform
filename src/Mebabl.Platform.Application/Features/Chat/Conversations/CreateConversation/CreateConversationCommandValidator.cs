using FluentValidation;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.CreateConversation;

public sealed class CreateConversationCommandValidator
    : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .When(x => x.Title is not null);

        RuleFor(x => x.IsGroup)
            .NotNull();
    }
}