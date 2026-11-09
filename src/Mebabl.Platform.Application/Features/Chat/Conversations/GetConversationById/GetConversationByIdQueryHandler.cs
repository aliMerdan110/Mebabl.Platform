using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.GetConversationById;

public sealed class GetConversationByIdQueryHandler
    : IRequestHandler<GetConversationByIdQuery, GetConversationByIdResponse>
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
        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        if (!_currentApplication.IsAuthenticated ||
            !_currentUser.IsAuthenticated ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty ||
            _currentUser.ApplicationId != applicationId)
        {
            throw new UnauthorizedAccessException(
                "An authenticated user of this application is required.");
        }

        if (request.ConversationId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid conversation ID is required.",
                nameof(request.ConversationId));
        }

        var conversation = await _context.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ConversationId &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (conversation is null)
        {
            throw new KeyNotFoundException(
                "Conversation not found.");
        }

        var isParticipant =
            await _context.ConversationParticipants
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.ConversationId == conversation.Id &&
                        x.UserId == userId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (!isParticipant)
        {
            // Do not reveal conversation details to non-participants.
            throw new KeyNotFoundException(
                "Conversation not found.");
        }

        var participantIds =
            await _context.ConversationParticipants
                .AsNoTracking()
                .Where(
                    x =>
                        x.ConversationId == conversation.Id &&
                        x.LeftAt == null)
                .OrderBy(x => x.JoinedAt)
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