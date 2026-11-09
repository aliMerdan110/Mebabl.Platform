using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AddReaction;

public sealed class AddReactionCommandHandler
    : IRequestHandler<AddReactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;
    private readonly ILogger<AddReactionCommandHandler> _logger;

    public AddReactionCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser,
        IRealtimePublisher realtimePublisher,
        ILogger<AddReactionCommandHandler> logger)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
        _realtimePublisher = realtimePublisher;
        _logger = logger;
    }

    public async Task Handle(
        AddReactionCommand request,
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

        var existingReaction = await _context.MessageReactions
            .FirstOrDefaultAsync(
                x =>
                    x.MessageId == message.Id &&
                    x.UserId == userId,
                cancellationToken);

        if (existingReaction is null)
        {
            _context.MessageReactions.Add(new MessageReaction
            {
                MessageId = message.Id,
                UserId = userId,
                Reaction = reactionValue
            });
        }
        else
        {
            existingReaction.Reaction = reactionValue;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Persist the reaction even if real-time delivery temporarily fails.
        try
        {
            using var payload = JsonDocument.Parse(
                JsonSerializer.Serialize(new
                {
                    messageId = message.Id,
                    conversationId = message.ConversationId,
                    userId,
                    reaction = reactionValue
                }));

            await _realtimePublisher.PublishAsync(
                applicationId,
                message.ConversationId,
                "reactionAdded",
                payload,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to publish reaction event for message {MessageId}.",
                message.Id);
        }
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