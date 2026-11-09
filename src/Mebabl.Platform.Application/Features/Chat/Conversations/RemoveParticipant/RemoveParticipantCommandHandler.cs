using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.RemoveParticipant;

public sealed class RemoveParticipantCommandHandler
    : IRequestHandler<RemoveParticipantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeConnectionManager _connectionManager;

    public RemoveParticipantCommandHandler(
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
        RemoveParticipantCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;
        var currentUserId = _currentUser.UserId;

        if (!_currentApplication.IsAuthenticated ||
            !_currentUser.IsAuthenticated ||
            applicationId == Guid.Empty ||
            currentUserId == Guid.Empty ||
            _currentUser.ApplicationId != applicationId)
        {
            throw new UnauthorizedAccessException(
                "An authenticated user of this application is required.");
        }

        if (request.ConversationId == Guid.Empty ||
            request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Valid conversation and participant IDs are required.");
        }

        if (request.UserId == currentUserId)
        {
            throw new InvalidOperationException(
                "Use the leave-conversation operation to remove yourself.");
        }

        var conversationExists = await _context.Conversations
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.ConversationId &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (!conversationExists)
        {
            throw new KeyNotFoundException(
                "Conversation not found in this application.");
        }

        var currentParticipant =
            await _context.ConversationParticipants
                .FirstOrDefaultAsync(
                    x =>
                        x.ConversationId == request.ConversationId &&
                        x.UserId == currentUserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (currentParticipant is null ||
            !currentParticipant.IsAdmin)
        {
            throw new UnauthorizedAccessException(
                "Only active conversation admins can remove participants.");
        }

        var targetParticipant =
            await _context.ConversationParticipants
                .FirstOrDefaultAsync(
                    x =>
                        x.ConversationId == request.ConversationId &&
                        x.UserId == request.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (targetParticipant is null)
        {
            throw new KeyNotFoundException(
                "Active participant not found.");
        }

        if (targetParticipant.IsAdmin)
        {
            var anotherAdminExists =
                await _context.ConversationParticipants
                    .AnyAsync(
                        x =>
                            x.ConversationId == request.ConversationId &&
                            x.UserId != request.UserId &&
                            x.LeftAt == null &&
                            x.IsAdmin,
                        cancellationToken);

            if (!anotherAdminExists)
            {
                throw new InvalidOperationException(
                    "The last active admin cannot be removed.");
            }
        }

        targetParticipant.LeftAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Remove every tracked connection belonging to the removed user
        // from the conversation after the membership change is persisted.
        await _connectionManager.RemoveUserFromConversationAsync(
            applicationId,
            request.UserId,
            request.ConversationId,
            CancellationToken.None);
    }
}