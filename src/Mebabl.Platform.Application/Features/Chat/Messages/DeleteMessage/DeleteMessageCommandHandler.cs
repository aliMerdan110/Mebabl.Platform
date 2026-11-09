
using System.Text.Json;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;

namespace Mebabl.Platform.Application.Features.Chat.Messages.DeleteMessage;

public sealed class DeleteMessageCommandHandler
    : IRequestHandler<DeleteMessageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;
    private readonly ILogger<DeleteMessageCommandHandler> _logger;

    public DeleteMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser,
        IRealtimePublisher realtimePublisher,
        ILogger<DeleteMessageCommandHandler> logger)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
        _realtimePublisher = realtimePublisher;
        _logger = logger;
    }

    public async Task Handle(
        DeleteMessageCommand request,
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

        if (request.MessageId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid message ID is required.",
                nameof(request.MessageId));
        }

        var message = await _context.Messages
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.MessageId &&
                    !x.IsDeleted,
                cancellationToken);

        if (message is null)
        {
            throw new KeyNotFoundException("Message not found.");
        }

        var isParticipant = await _context.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ConversationId == message.ConversationId &&
                    x.UserId == userId &&
                    x.LeftAt == null &&
                    x.Conversation.ApplicationId == applicationId &&
                    !x.Conversation.IsDeleted,
                cancellationToken);

        if (!isParticipant)
        {
            throw new KeyNotFoundException(
                "Message not found or access is unavailable.");
        }

        if (message.SenderId != userId)
        {
            throw new UnauthorizedAccessException(
                "Only the message sender can delete it.");
        }

        var now = DateTime.UtcNow;

        message.IsDeleted = true;
        message.DeletedAt = now;
        message.DeletedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        using var payload = JsonDocument.Parse(
            JsonSerializer.Serialize(new
            {
                message.Id,
                message.ConversationId,
                deletedBy = userId,
                deletedAt = now
            }));

        try
        {
            await _realtimePublisher.PublishAsync(
                applicationId,
                message.ConversationId,
                "messageDeleted",
                payload,
                cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Message {MessageId} was deleted, but realtime publication failed.",
                message.Id);
        }
    }
}
