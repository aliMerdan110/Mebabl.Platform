
using System.Text.Json;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;

namespace Mebabl.Platform.Application.Features.Chat.Messages.EditMessage;

public sealed class EditMessageCommandHandler
    : IRequestHandler<EditMessageCommand, EditMessageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;
    private readonly ILogger<EditMessageCommandHandler> _logger;

    public EditMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser,
        IRealtimePublisher realtimePublisher,
        ILogger<EditMessageCommandHandler> logger)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
        _realtimePublisher = realtimePublisher;
        _logger = logger;
    }

    public async Task<EditMessageResponse> Handle(
        EditMessageCommand request,
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

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException(
                "Message content is required.",
                nameof(request.Content));
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
                "Only the message sender can edit it.");
        }

        var now = DateTime.UtcNow;

        message.Content = request.Content.Trim();
        message.IsEdited = true;
        message.EditedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        using var payload = JsonDocument.Parse(
            JsonSerializer.Serialize(new
            {
                message.Id,
                message.ConversationId,
                message.SenderId,
                message.Content,
                message.MessageType,
                message.IsEdited,
                message.EditedAt
            }));

        try
        {
            await _realtimePublisher.PublishAsync(
                applicationId,
                message.ConversationId,
                "messageUpdated",
                payload,
                cancellationToken);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Message {MessageId} was updated, but realtime publication failed.",
                message.Id);
        }

        return new EditMessageResponse(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.Content,
            message.MessageType,
            message.IsEdited,
            message.EditedAt);
    }
}
