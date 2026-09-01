using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;
using Mebabl.Platform.Domain.Modules.Chat.Entities;
using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AddReaction;

public sealed class AddReactionCommandHandler
    : IRequestHandler<AddReactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;

    public AddReactionCommandHandler(
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
        AddReactionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

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

        var existingReaction =
            await _context.MessageReactions
                .FirstOrDefaultAsync(
                    x =>
                        x.MessageId == request.MessageId &&
                        x.UserId == _currentUser.UserId,
                    cancellationToken);

        if (existingReaction is not null)
        {
            existingReaction.Reaction = request.Reaction;
        }
        else
        {
            _context.MessageReactions.Add(
                new MessageReaction
                {
                    MessageId = message.Id,
                    UserId = _currentUser.UserId,
                    Reaction = request.Reaction
                });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var payload = JsonDocument.Parse(
            JsonSerializer.Serialize(new
            {
                messageId = message.Id,
                userId = _currentUser.UserId,
                reaction = request.Reaction
            }));

        await _realtimePublisher.PublishAsync(
            _currentApplication.ApplicationId,
            message.ConversationId,
            "reactionAdded",
            payload,
            cancellationToken);
    }
}