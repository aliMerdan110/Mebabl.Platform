using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Messages.DeleteMessageAttachment;

public sealed class DeleteMessageAttachmentCommandHandler
    : IRequestHandler<DeleteMessageAttachmentCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public DeleteMessageAttachmentCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        DeleteMessageAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        if (request.AttachmentId == Guid.Empty)
            throw new ArgumentException("Attachment ID is required.");

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var attachment = await _context.MessageAttachments
            .Include(x => x.Message)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.AttachmentId &&
                    !x.IsDeleted &&
                    !x.Message.IsDeleted &&
                    _context.Conversations.Any(c =>
                        c.Id == x.Message.ConversationId &&
                        c.ApplicationId == applicationId &&
                        !c.IsDeleted),
                cancellationToken);

        if (attachment is null)
            throw new KeyNotFoundException("Attachment not found.");

        if (attachment.Message.SenderId != userId)
        {
            throw new UnauthorizedAccessException(
                "Only the message sender can remove this attachment.");
        }

        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(
                x =>
                    x.ConversationId == attachment.Message.ConversationId &&
                    x.UserId == userId &&
                    x.LeftAt == null,
                cancellationToken);

        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "User is not an active conversation participant.");

        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTime.UtcNow;
        attachment.DeletedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private void EnsureAuthenticated()
    {
        if (!_currentApplication.IsAuthenticated ||
            !_currentUser.IsAuthenticated ||
            _currentApplication.ApplicationId == Guid.Empty ||
            _currentUser.ApplicationId != _currentApplication.ApplicationId ||
            _currentUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }
    }
}