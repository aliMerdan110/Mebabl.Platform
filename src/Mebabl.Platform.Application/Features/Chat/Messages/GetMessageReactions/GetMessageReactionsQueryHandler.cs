using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessageReactions;

public sealed class GetMessageReactionsQueryHandler
    : IRequestHandler<
        GetMessageReactionsQuery,
        IReadOnlyList<GetMessageReactionsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetMessageReactionsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GetMessageReactionsResponse>> Handle(
        GetMessageReactionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var message = await _context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.MessageId,
                cancellationToken);

        if (message is null)
            throw new KeyNotFoundException(
                "Message not found.");

        var participantExists =
            await _context.ConversationParticipants
                .AnyAsync(
                    x =>
                        x.ConversationId == message.ConversationId &&
                        x.UserId == _currentUser.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (!participantExists)
            throw new UnauthorizedAccessException(
                "User is not a participant.");

        return await _context.MessageReactions
            .AsNoTracking()
            .Where(x => x.MessageId == request.MessageId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new GetMessageReactionsResponse(
                x.Id,
                x.MessageId,
                x.UserId,
                x.Reaction,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}