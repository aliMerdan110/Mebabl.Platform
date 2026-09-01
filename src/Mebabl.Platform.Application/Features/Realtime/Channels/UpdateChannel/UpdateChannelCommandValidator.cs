using FluentValidation;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.UpdateChannel;

public sealed class UpdateChannelCommandValidator
    : AbstractValidator<UpdateChannelCommand>
{
    public UpdateChannelCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}