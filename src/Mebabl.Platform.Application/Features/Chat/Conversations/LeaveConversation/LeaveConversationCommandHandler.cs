using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.LeaveConversation;

public sealed class LeaveConversationCommandHandler
    : IRequestHandler<LeaveConversationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeConnectionManager _connectionManager;

    public LeaveConversationCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser,
        IRealtimeConnectionManager connectionManager)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
        _connectionManager = connectionManager;
    }

    public async Task Handle(
        LeaveConversationCommand request,
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

        var participant =
            await _context.ConversationParticipants
                .FirstOrDefaultAsync(
                    x =>
                        x.ConversationId == request.ConversationId &&
                        x.UserId == userId &&
                        x.LeftAt == null &&
                        x.Conversation.ApplicationId == applicationId &&
                        !x.Conversation.IsDeleted,
                    cancellationToken);

        if (participant is null)
        {
            throw new KeyNotFoundException(
                "Active conversation membership not found.");
        }

        if (participant.IsAdmin)
        {
            var anotherAdminExists =
                await _context.ConversationParticipants
                    .AnyAsync(
                        x =>
                            x.ConversationId == request.ConversationId &&
                            x.UserId != userId &&
                            x.LeftAt == null &&
                            x.IsAdmin,
                        cancellationToken);

            if (!anotherAdminExists)
            {
                var replacementAdmin =
                    await _context.ConversationParticipants
                        .Where(
                            x =>
                                x.ConversationId == request.ConversationId &&
                                x.UserId != userId &&
                                x.LeftAt == null)
                        .OrderBy(x => x.JoinedAt)
                        .ThenBy(x => x.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                if (replacementAdmin is not null)
                {
                    replacementAdmin.IsAdmin = true;
                }
            }
        }

        participant.LeftAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Membership is now revoked in the database. Remove all tracked
        // connections for this user from this conversation's SignalR group.
        await _connectionManager.RemoveUserFromConversationAsync(
            applicationId,
            userId,
            request.ConversationId,
            CancellationToken.None);
    }
}