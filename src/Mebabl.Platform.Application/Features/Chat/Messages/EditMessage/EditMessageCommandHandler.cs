using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;

namespace Mebabl.Platform.Application.Features.Chat.Messages.EditMessage;

public sealed class EditMessageCommandHandler
    : IRequestHandler<
        EditMessageCommand,
        EditMessageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;

    public EditMessageCommandHandler(
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

    public async Task<EditMessageResponse> Handle(
        EditMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException(
                "Message content is required.");

        var message = await _context.Messages
            .FirstOrDefaultAsync(
                x => x.Id == request.MessageId,
                cancellationToken);

        if (message is null)
            throw new KeyNotFoundException(
                "Message not found.");

        if (message.SenderId != _currentUser.UserId)
            throw new UnauthorizedAccessException(
                "Only the message sender can edit it.");

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

        message.Content = request.Content.Trim();
        message.IsEdited = true;
        message.EditedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var payload = JsonDocument.Parse(
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

        await _realtimePublisher.PublishAsync(
            message.ConversationId,
            message.Id,
            "messageUpdated",
            payload,
            cancellationToken);

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