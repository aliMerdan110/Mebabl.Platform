using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

    public DeleteMessageCommandHandler(
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
        DeleteMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var message = await _context.Messages
            .FirstOrDefaultAsync(
                x => x.Id == request.MessageId &&
                     !x.IsDeleted,
                cancellationToken);

        if (message is null)
            throw new KeyNotFoundException(
                "Message not found.");

        if (message.SenderId != _currentUser.UserId)
            throw new UnauthorizedAccessException(
                "Only the message sender can delete it.");

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

        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        message.DeletedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        var payload = JsonDocument.Parse(
            JsonSerializer.Serialize(new
            {
                message.Id,
                message.ConversationId,
                deletedBy = message.DeletedBy,
                deletedAt = message.DeletedAt
            }));

        await _realtimePublisher.PublishAsync(
            message.ConversationId,
            message.Id,
            "messageDeleted",
            payload,
            cancellationToken);
    }
}