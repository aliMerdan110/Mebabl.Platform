using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.EditMessage;

public sealed record EditMessageCommand(
    Guid MessageId,
    string Content
) : IRequest<EditMessageResponse>;