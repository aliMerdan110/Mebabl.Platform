using FluentValidation;

namespace Mebabl.Platform.Application.Features.Chat.Messages.SendMessage;

public sealed class SendMessageCommandValidator
    : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(10000);

        RuleFor(x => x.MessageType)
            .MaximumLength(50)
            .When(x => x.MessageType is not null);
    }
}