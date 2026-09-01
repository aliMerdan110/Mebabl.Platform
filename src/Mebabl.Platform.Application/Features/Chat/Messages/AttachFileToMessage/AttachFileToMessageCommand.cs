using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AttachFileToMessage;

public sealed record AttachFileToMessageCommand(
    Guid MessageId,
    Guid StoredFileId,
    string? Caption
) : IRequest<AttachFileToMessageResponse>;