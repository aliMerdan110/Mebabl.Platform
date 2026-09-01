using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AddMessageReaction;

public sealed class AddMessageReactionCommandHandler
    : IRequestHandler<
        AddMessageReactionCommand,
        AddMessageReactionResponse>
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
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        if (string.IsNullOrWhiteSpace(request.Reaction))
            throw new ArgumentException(
                "Reaction is required.");

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

        var existingReaction =
            await _context.MessageReactions
                .FirstOrDefaultAsync(
                    x =>
                        x.MessageId == request.MessageId &&
                        x.UserId == _currentUser.UserId &&
                        x.Reaction == request.Reaction,
                    cancellationToken);

        if (existingReaction is not null)
            throw new InvalidOperationException(
                "Reaction already exists.");

        var reaction = new MessageReaction
        {
            MessageId = request.MessageId,
            UserId = _currentUser.UserId,
            Reaction = request.Reaction.Trim()
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
}