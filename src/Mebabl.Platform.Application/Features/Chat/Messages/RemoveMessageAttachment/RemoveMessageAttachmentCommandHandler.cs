using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Messages.RemoveMessageAttachment;

public sealed class RemoveMessageAttachmentCommandHandler
    : IRequestHandler<RemoveMessageAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public RemoveMessageAttachmentCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        RemoveMessageAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var attachment = await _context.MessageAttachments
            .Include(x => x.Message)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.MessageAttachmentId &&
                    !x.IsDeleted,
                cancellationToken);

        if (attachment is null)
            throw new KeyNotFoundException(
                "Message attachment not found.");

        if (attachment.Message.IsDeleted)
            throw new KeyNotFoundException(
                "Message not found.");

        if (attachment.Message.SenderId != _currentUser.UserId)
            throw new UnauthorizedAccessException(
                "Only the message sender can remove attachments.");

        var participantExists =
            await _context.ConversationParticipants
                .AnyAsync(
                    x =>
                        x.ConversationId ==
                            attachment.Message.ConversationId &&
                        x.UserId == _currentUser.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (!participantExists)
            throw new UnauthorizedAccessException(
                "User is not a participant.");

        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTime.UtcNow;
        attachment.DeletedBy = _currentUser.UserId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}