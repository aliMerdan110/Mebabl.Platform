using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Messages.RemoveMessageReaction;

public sealed class RemoveMessageReactionCommandHandler
    : IRequestHandler<RemoveMessageReactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public RemoveMessageReactionCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        RemoveMessageReactionCommand request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        if (request.MessageId == Guid.Empty)
            throw new ArgumentException("Message ID is required.");

        var reactionValue = request.Reaction?.Trim();

        if (string.IsNullOrWhiteSpace(reactionValue))
            throw new ArgumentException("Reaction is required.");

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
            .AnyAsync(
                x =>
                    x.ConversationId == message.ConversationId &&
                    x.UserId == userId &&
                    x.LeftAt == null,
                cancellationToken);

        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "User is not an active conversation participant.");

        var reaction = await _context.MessageReactions
            .FirstOrDefaultAsync(
                x =>
                    x.MessageId == message.Id &&
                    x.UserId == userId &&
                    x.Reaction == reactionValue,
                cancellationToken);

        if (reaction is null)
            throw new KeyNotFoundException("Reaction not found.");

        _context.MessageReactions.Remove(reaction);

        await _context.SaveChangesAsync(cancellationToken);
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