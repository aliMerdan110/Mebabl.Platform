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
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var attachment = await _context.MessageAttachments
            .FirstOrDefaultAsync(
                x => x.Id == request.AttachmentId,
                cancellationToken);

        if (attachment is null)
            throw new KeyNotFoundException(
                "Attachment not found.");

        var message = await _context.Messages
            .FirstOrDefaultAsync(
                x => x.Id == attachment.MessageId,
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

        _context.MessageAttachments.Remove(attachment);

        await _context.SaveChangesAsync(cancellationToken);
    }
}