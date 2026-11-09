using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;

namespace Mebabl.Platform.Application.Features.Chat.Messages.RemoveReaction;

public sealed class RemoveReactionCommandHandler
    : IRequestHandler<RemoveReactionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;
    private readonly ILogger<RemoveReactionCommandHandler> _logger;

    public RemoveReactionCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser,
        IRealtimePublisher realtimePublisher,
        ILogger<RemoveReactionCommandHandler> logger)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
        _realtimePublisher = realtimePublisher;
        _logger = logger;
    }

    public async Task Handle(
        RemoveReactionCommand request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        if (request.MessageId == Guid.Empty)
            throw new ArgumentException("Message ID is required.");

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var message = await (
            from chatMessage in _context.Messages.AsNoTracking()
            join conversation in _context.Conversations.AsNoTracking()
                on chatMessage.ConversationId equals conversation.Id
            where chatMessage.Id == request.MessageId
                  && !chatMessage.IsDeleted
                  && conversation.ApplicationId == applicationId
                  && !conversation.IsDeleted
            select new
            {
                chatMessage.Id,
                chatMessage.ConversationId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (message is null)
            throw new KeyNotFoundException("Message not found.");

        var isParticipant = await _context.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ConversationId == message.ConversationId &&
                    x.UserId == userId &&
                    x.LeftAt == null,
                cancellationToken);

        if (!isParticipant)
        {
            throw new UnauthorizedAccessException(
                "User is not an active conversation participant.");
        }

        var reaction = await _context.MessageReactions
            .FirstOrDefaultAsync(
                x =>
                    x.MessageId == message.Id &&
                    x.UserId == userId,
                cancellationToken);

        // الإزالة قابلة للتكرار؛ عدم وجود تفاعل لا يتطلب تعديلًا.
        if (reaction is null)
            return;

        _context.MessageReactions.Remove(reaction);

        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            using var payload = JsonDocument.Parse(
                JsonSerializer.Serialize(new
                {
                    messageId = message.Id,
                    conversationId = message.ConversationId,
                    userId
                }));

            await _realtimePublisher.PublishAsync(
                applicationId,
                message.ConversationId,
                "reactionRemoved",
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
            _logger.LogWarning(
                exception,
                "Reaction was removed, but realtime notification failed for message {MessageId}.",
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
            throw new UnauthorizedAccessException(
                "An authenticated user in the current application is required.");
        }
    }
}