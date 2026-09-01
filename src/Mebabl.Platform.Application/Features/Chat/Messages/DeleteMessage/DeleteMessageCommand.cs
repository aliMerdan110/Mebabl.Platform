using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.DeleteMessage;

public sealed record DeleteMessageCommand(
    Guid MessageId
) : IRequest;