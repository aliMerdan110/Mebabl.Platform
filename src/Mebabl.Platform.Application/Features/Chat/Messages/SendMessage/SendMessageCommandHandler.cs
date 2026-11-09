
using System.Text.Json;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Messages.SendMessage;

public sealed class SendMessageCommandHandler
    : IRequestHandler<SendMessageCommand, SendMessageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser,
        IRealtimePublisher realtimePublisher,
        ILogger<SendMessageCommandHandler> logger)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
        _realtimePublisher = realtimePublisher;
        _logger = logger;
    }

    public async Task<SendMessageResponse> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        if (!_currentApplication.IsAuthenticated ||
            !_currentUser.IsAuthenticated ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty ||
            _currentUser.ApplicationId != applicationId)
        {
            throw new UnauthorizedAccessException(
                "An authenticated user of this application is required.");
        }

        if (request.ConversationId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid conversation ID is required.",
                nameof(request.ConversationId));
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException(
                "Message content is required.",
                nameof(request.Content));
        }

        var isParticipant = await _context.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ConversationId == request.ConversationId &&
                    x.UserId == userId &&
                    x.LeftAt == null &&
                    x.Conversation.ApplicationId == applicationId &&
                    !x.Conversation.IsDeleted,
                cancellationToken);

        if (!isParticipant)
        {
            throw new UnauthorizedAccessException(
                "Active membership in this application's conversation is required.");
        }

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = userId,
            Content = request.Content.Trim(),
            MessageType = request.MessageType
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync(cancellationToken);

        using var payload = JsonDocument.Parse(
            JsonSerializer.Serialize(new
            {
                message.Id,
                message.ConversationId,
                message.SenderId,
                message.Content,
                message.MessageType,
                message.CreatedAt
            }));

        // The publisher expects applicationId, then conversationId.
        try
        {
            await _realtimePublisher.PublishAsync(
                applicationId,
                message.ConversationId,
                "messageReceived",
                payload,
                cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException)
        {
            // The message is already persisted. Log delivery failure
            // rather than reporting that message creation failed.
            _logger.LogWarning(
                exception,
                "Message {MessageId} was saved, but realtime publication failed.",
                message.Id);
        }

        return new SendMessageResponse(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.Content,
            message.MessageType,
            message.CreatedAt);
    }
}
