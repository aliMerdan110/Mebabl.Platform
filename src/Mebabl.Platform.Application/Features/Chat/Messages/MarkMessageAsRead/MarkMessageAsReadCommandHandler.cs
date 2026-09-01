using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Messages.MarkMessageAsRead;

public sealed class MarkMessageAsReadCommandHandler
    : IRequestHandler<MarkMessageAsReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public MarkMessageAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        MarkMessageAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var message = await _context.Messages
            .FirstOrDefaultAsync(
                x => x.Id == request.MessageId,
                cancellationToken);

        if (message is null)
            throw new KeyNotFoundException(
                "Message not found.");

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

        var existingRead = await _context.MessageReads
            .FirstOrDefaultAsync(
                x =>
                    x.MessageId == request.MessageId &&
                    x.UserId == _currentUser.UserId,
                cancellationToken);

        if (existingRead is not null)
            return;

        var messageRead = new MessageRead
        {
            MessageId = message.Id,
            UserId = _currentUser.UserId,
            ReadAt = DateTime.UtcNow
        };

        _context.MessageReads.Add(messageRead);

        await _context.SaveChangesAsync(cancellationToken);
    }
}