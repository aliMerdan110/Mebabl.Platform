using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Realtime;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Messages.SendMessage;

public sealed class SendMessageCommandHandler
    : IRequestHandler<
        SendMessageCommand,
        SendMessageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimePublisher _realtimePublisher;

    public SendMessageCommandHandler(
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

    public async Task<SendMessageResponse> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var participantExists =
            await _context.ConversationParticipants
                .AnyAsync(
                    x =>
                        x.ConversationId == request.ConversationId &&
                        x.UserId == _currentUser.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (!participantExists)
            throw new UnauthorizedAccessException(
                "User is not a participant.");

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = _currentUser.UserId,
            Content = request.Content,
            MessageType = request.MessageType
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync(cancellationToken);

        var payload = JsonDocument.Parse(
    JsonSerializer.Serialize(new
    {
        message.Id,
        message.ConversationId,
        message.SenderId,
        message.Content,
        message.MessageType,
        message.CreatedAt
    }));

await _realtimePublisher.PublishAsync(
    request.ConversationId,
    request.ConversationId,
    "messageReceived",
    payload,
    cancellationToken);

        return new SendMessageResponse(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.Content,
            message.MessageType,
            message.CreatedAt);
    }
}