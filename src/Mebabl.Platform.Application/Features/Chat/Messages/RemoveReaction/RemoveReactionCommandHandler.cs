using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;
using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Chat.Messages.RemoveReaction;

public sealed class RemoveReactionCommandHandler
    : IRequestHandler<RemoveReactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;

    public RemoveReactionCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser,
        IRealtimePublisher realtimePublisher)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
        _realtimePublisher = realtimePublisher;
    }

    public async Task Handle(
        RemoveReactionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var reaction = await _context.MessageReactions
            .FirstOrDefaultAsync(
                x =>
                    x.MessageId == request.MessageId &&
                    x.UserId == _currentUser.UserId,
                cancellationToken);

        if (reaction is null)
            return;

        var message = await _context.Messages
            .FirstOrDefaultAsync(
                x => x.Id == request.MessageId,
                cancellationToken);

        if (message is null)
            throw new KeyNotFoundException("Message not found.");

        var participantExists =
            await _context.ConversationParticipants
                .AnyAsync(
                    x =>
                        x.ConversationId == message.ConversationId &&
                        x.UserId == _currentUser.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (!participantExists)
            throw new UnauthorizedAccessException();

        _context.MessageReactions.Remove(reaction);

        await _context.SaveChangesAsync(cancellationToken);

        var payload = JsonDocument.Parse(
            JsonSerializer.Serialize(new
            {
                messageId = message.Id,
                userId = _currentUser.UserId
            }));

        await _realtimePublisher.PublishAsync(
            _currentApplication.ApplicationId,
            message.ConversationId,
            "reactionRemoved",
            payload,
            cancellationToken);
    }
}