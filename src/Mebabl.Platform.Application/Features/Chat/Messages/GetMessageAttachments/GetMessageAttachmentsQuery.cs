using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessageAttachments;

public sealed record GetMessageAttachmentsQuery(
    Guid MessageId
) : IRequest<IReadOnlyList<GetMessageAttachmentsResponse>>;