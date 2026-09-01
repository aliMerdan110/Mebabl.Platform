using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.RemoveMessageAttachment;

public sealed record RemoveMessageAttachmentCommand(
    Guid MessageAttachmentId
) : IRequest;