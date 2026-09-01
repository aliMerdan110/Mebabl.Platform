using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.DeleteMessageAttachment;

public sealed record DeleteMessageAttachmentCommand(
    Guid AttachmentId
) : IRequest;