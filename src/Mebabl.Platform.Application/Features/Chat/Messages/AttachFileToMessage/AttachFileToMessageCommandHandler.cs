using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Messages.AttachFileToMessage;

public sealed class AttachFileToMessageCommandHandler
    : IRequestHandler<
        AttachFileToMessageCommand,
        AttachFileToMessageResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public AttachFileToMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<AttachFileToMessageResponse> Handle(
        AttachFileToMessageCommand request,
        CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        if (request.MessageId == Guid.Empty ||
            request.StoredFileId == Guid.Empty)
        {
            throw new ArgumentException(
                "Message ID and stored file ID are required.");
        }

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var message = await _context.Messages
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.MessageId &&
                    !x.IsDeleted &&
                    _context.Conversations.Any(c =>
                        c.Id == x.ConversationId &&
                        c.ApplicationId == applicationId &&
                        !c.IsDeleted),
                cancellationToken);

        if (message is null)
            throw new KeyNotFoundException("Message not found.");

        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(
                x =>
                    x.ConversationId == message.ConversationId &&
                    x.UserId == userId &&
                    x.LeftAt == null,
                cancellationToken);

        if (!isParticipant)
            throw new UnauthorizedAccessException(
                "User is not an active conversation participant.");

        var storedFile = await _context.StoredFiles
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.StoredFileId &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (storedFile is null)
            throw new KeyNotFoundException(
                "Stored file was not found in this application.");

        var alreadyAttached = await _context.MessageAttachments
            .AnyAsync(
                x =>
                    x.MessageId == message.Id &&
                    x.StoredFileId == storedFile.Id &&
                    !x.IsDeleted,
                cancellationToken);

        if (alreadyAttached)
            throw new InvalidOperationException(
                "File is already attached to this message.");

        var attachment = new MessageAttachment
        {
            MessageId = message.Id,
            StoredFileId = storedFile.Id,
            Caption = request.Caption?.Trim()
        };

        _context.MessageAttachments.Add(attachment);

        await _context.SaveChangesAsync(cancellationToken);

        return new AttachFileToMessageResponse(
            attachment.Id,
            attachment.MessageId,
            attachment.StoredFileId,
            attachment.Caption,
            attachment.CreatedAt);
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