using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AddMessageReaction;

public sealed class AddMessageReactionCommandHandler
    : IRequestHandler<AddMessageReactionCommand, AddMessageReactionResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public AddMessageReactionCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<AddMessageReactionResponse> Handle(
        AddMessageReactionCommand request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        if (request.MessageId == Guid.Empty)
            throw new ArgumentException("Message ID is required.");

        var reactionValue = request.Reaction?.Trim();

        if (string.IsNullOrWhiteSpace(reactionValue))
            throw new ArgumentException("Reaction is required.");

        if (reactionValue.Length > 64)
            throw new ArgumentException(
                "Reaction cannot exceed 64 characters.");

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

        var alreadyExists = await _context.MessageReactions
            .AnyAsync(
                x =>
                    x.MessageId == message.Id &&
                    x.UserId == userId &&
                    x.Reaction == reactionValue,
                cancellationToken);

        if (alreadyExists)
            throw new InvalidOperationException(
                "Reaction already exists.");

        var reaction = new MessageReaction
        {
            MessageId = message.Id,
            UserId = userId,
            Reaction = reactionValue
        };

        _context.MessageReactions.Add(reaction);

        await _context.SaveChangesAsync(cancellationToken);

        return new AddMessageReactionResponse(
            reaction.Id,
            reaction.MessageId,
            reaction.UserId,
            reaction.Reaction,
            reaction.CreatedAt);
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