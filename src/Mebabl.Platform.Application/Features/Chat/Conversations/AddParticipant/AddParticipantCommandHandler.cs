using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.AddParticipant;

public sealed class AddParticipantCommandHandler
    : IRequestHandler<AddParticipantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public AddParticipantCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        AddParticipantCommand request,
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

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ConversationId &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (conversation is null)
        {
            throw new KeyNotFoundException(
                "Conversation not found in this application.");
        }

        var currentParticipant =
            await _context.ConversationParticipants
                .FirstOrDefaultAsync(
                    x =>
                        x.ConversationId == conversation.Id &&
                        x.UserId == currentUserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (currentParticipant is null ||
            !currentParticipant.IsAdmin)
        {
            throw new UnauthorizedAccessException(
                "Only active conversation admins can add participants.");
        }

        var targetUserIsActive = await _context.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.UserId &&
                    x.ApplicationId == applicationId &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    !x.Account.IsDeleted &&
                    x.Application.IsActive &&
                    !x.Application.IsDeleted,
                cancellationToken);

        if (!targetUserIsActive)
        {
            throw new KeyNotFoundException(
                "Participant not found in this application.");
        }

        var existingParticipant =
            await _context.ConversationParticipants
                .FirstOrDefaultAsync(
                    x =>
                        x.ConversationId == conversation.Id &&
                        x.UserId == request.UserId,
                    cancellationToken);

        if (existingParticipant is not null &&
            existingParticipant.LeftAt is null)
        {
            return;
        }

        // Direct conversations may contain at most two active participants.
        if (!conversation.IsGroup)
        {
            var activeParticipantCount =
                await _context.ConversationParticipants
                    .CountAsync(
                        x =>
                            x.ConversationId == conversation.Id &&
                            x.LeftAt == null,
                        cancellationToken);

            if (activeParticipantCount >= 2)
            {
                throw new InvalidOperationException(
                    "A direct conversation cannot have more than two active participants.");
            }
        }

        var now = DateTime.UtcNow;

        if (existingParticipant is not null)
        {
            // Reuse the existing row because (ConversationId, UserId)
            // has a unique index.
            existingParticipant.JoinedAt = now;
            existingParticipant.LeftAt = null;
            existingParticipant.IsAdmin = request.IsAdmin;
        }
        else
        {
            _context.ConversationParticipants.Add(
                new ConversationParticipant
                {
                    ConversationId = conversation.Id,
                    UserId = request.UserId,
                    JoinedAt = now,
                    IsAdmin = request.IsAdmin
                });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}