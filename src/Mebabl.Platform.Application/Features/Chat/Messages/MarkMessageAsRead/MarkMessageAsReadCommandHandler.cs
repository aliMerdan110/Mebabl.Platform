
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
            .AsNoTracking()
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

        var alreadyRead = await _context.MessageReads
            .AnyAsync(
                x =>
                    x.MessageId == request.MessageId &&
                    x.UserId == userId,
                cancellationToken);

        if (alreadyRead)
        {
            return;
        }

        _context.MessageReads.Add(
            new MessageRead
            {
                MessageId = message.Id,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
