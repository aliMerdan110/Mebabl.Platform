using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.GetConversations;

public sealed class GetConversationsQueryHandler
    : IRequestHandler<
        GetConversationsQuery,
        IReadOnlyList<GetConversationsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetConversationsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GetConversationsResponse>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var conversationIds =
            _context.ConversationParticipants
                .AsNoTracking()
                .Where(x =>
                    x.UserId == _currentUser.UserId &&
                    x.LeftAt == null)
                .Select(x => x.ConversationId);

        return await _context.Conversations
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId ==
                    _currentApplication.ApplicationId &&
                conversationIds.Contains(x.Id))
            .OrderByDescending(x => x.CreatedAt)
            .Skip(request.Offset)
            .Take(request.Limit)
            .Select(x => new GetConversationsResponse(
                x.Id,
                x.Title,
                x.IsGroup,
                x.CreatedAt,
                x.Participants
                    .Where(p => p.LeftAt == null)
                    .Select(p => p.UserId)
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}