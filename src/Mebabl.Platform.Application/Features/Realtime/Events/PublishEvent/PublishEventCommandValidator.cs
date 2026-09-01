using FluentValidation;

namespace Mebabl.Platform.Application.Features.Realtime.Events.PublishEvent;

public sealed class PublishEventCommandValidator
    : AbstractValidator<PublishEventCommand>
{
    public PublishEventCommandValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Payload)
            .NotNull();
    }
}