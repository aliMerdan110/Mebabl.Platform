using FluentValidation;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.CreateChannel;

public sealed class CreateChannelCommandValidator
    : AbstractValidator<CreateChannelCommand>
{
    public CreateChannelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}