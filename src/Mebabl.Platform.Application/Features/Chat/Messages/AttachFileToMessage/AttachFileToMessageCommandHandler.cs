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
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var message = await _context.Messages
    .FirstOrDefaultAsync(
        x =>
            x.Id == request.MessageId &&
            !x.IsDeleted,
        cancellationToken);

        if (message is null)
            throw new KeyNotFoundException("Message not found.");

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

        var storedFile = await _context.StoredFiles
    .FirstOrDefaultAsync(
        x =>
            x.Id == request.StoredFileId &&
            !x.IsDeleted,
        cancellationToken);

        if (storedFile is null)
            throw new KeyNotFoundException("Stored file not found.");

        var alreadyAttached =
            await _context.MessageAttachments
                .AnyAsync(
                    x =>
                        x.MessageId == request.MessageId &&
                        x.StoredFileId == request.StoredFileId,
                    cancellationToken);

        if (alreadyAttached)
            throw new InvalidOperationException(
                "File is already attached to this message.");

        var attachment = new MessageAttachment
        {
            MessageId = message.Id,
            StoredFileId = storedFile.Id,
            Caption = request.Caption
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
}