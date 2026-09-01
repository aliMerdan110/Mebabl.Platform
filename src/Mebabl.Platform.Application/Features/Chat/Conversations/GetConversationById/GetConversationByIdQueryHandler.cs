using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.GetConversationById;

public sealed class GetConversationByIdQueryHandler
    : IRequestHandler<
        GetConversationByIdQuery,
        GetConversationByIdResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetConversationByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<GetConversationByIdResponse> Handle(
        GetConversationByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var isParticipant =
            await _context.ConversationParticipants
                .AnyAsync(
                    x =>
                        x.ConversationId == request.ConversationId &&
                        x.UserId == _currentUser.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (!isParticipant)
            throw new UnauthorizedAccessException();

        var conversation =
            await _context.Conversations
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.ConversationId,
                    cancellationToken);

        if (conversation is null)
            throw new KeyNotFoundException(
                "Conversation not found.");

        var participantIds =
            await _context.ConversationParticipants
                .AsNoTracking()
                .Where(
                    x =>
                        x.ConversationId == conversation.Id &&
                        x.LeftAt == null)
                .Select(x => x.UserId)
                .ToListAsync(cancellationToken);

        return new GetConversationByIdResponse(
    conversation.Id,
    conversation.Title,
    conversation.IsGroup,
    conversation.CreatedAt,
    participantIds);
    }
}