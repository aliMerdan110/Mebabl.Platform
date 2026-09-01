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

        var reaction = await _context.MessageReactions
            .FirstOrDefaultAsync(
                x =>
                    x.MessageId == request.MessageId &&
                    x.UserId == _currentUser.UserId &&
                    x.Reaction == request.Reaction,
                cancellationToken);

        if (reaction is null)
            throw new KeyNotFoundException(
                "Reaction not found.");

        _context.MessageReactions.Remove(reaction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}