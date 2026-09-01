using MediatR;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessageReads;

public sealed record GetMessageReadsQuery(
    Guid MessageId
) : IRequest<IReadOnlyList<GetMessageReadsResponse>>;