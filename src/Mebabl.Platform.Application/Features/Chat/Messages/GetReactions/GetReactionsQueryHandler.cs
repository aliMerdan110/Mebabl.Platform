using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetReactions;

public sealed class GetReactionsQueryHandler
    : IRequestHandler<
        GetReactionsQuery,
        IReadOnlyList<GetReactionsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetReactionsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GetReactionsResponse>> Handle(
        GetReactionsQuery request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        if (request.MessageId == Guid.Empty)
            throw new ArgumentException("Message ID is required.");

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var message = await _context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.MessageId &&
                    !x.IsDeleted &&
                    _context.Conversations.Any(c =>
                        c.Id == x.ConversationId &&
                        c.ApplicationId == applicationId &&
                        !c.IsDeleted),
                cancellationToken);

        if (message is null)
            throw new KeyNotFoundException("Message not found.");

        var isParticipant = await _context.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ConversationId == message.ConversationId &&
                    x.UserId == userId &&
                    x.LeftAt == null,
                cancellationToken);

        if (!isParticipant)
        {
            throw new UnauthorizedAccessException(
                "User is not an active conversation participant.");
        }

        return await _context.MessageReactions
            .AsNoTracking()
            .Where(x => x.MessageId == message.Id)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new GetReactionsResponse(
                x.Id,
                x.MessageId,
                x.UserId,
                x.Reaction,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    private void EnsureAuthenticated()
    {
        if (!_currentApplication.IsAuthenticated ||
            !_currentUser.IsAuthenticated ||
            _currentApplication.ApplicationId == Guid.Empty ||
            _currentUser.ApplicationId != _currentApplication.ApplicationId ||
            _currentUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }
    }
}