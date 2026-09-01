using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.MarkMessageAsRead;

public sealed record MarkMessageAsReadCommand(
    Guid MessageId
) : IRequest;