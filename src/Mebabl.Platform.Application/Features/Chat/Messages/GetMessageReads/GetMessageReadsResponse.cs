namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessageReads;

public sealed record GetMessageReadsResponse(
    Guid Id,
    Guid MessageId,
    Guid UserId,
    DateTime ReadAt
);