using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessageAttachments;

public sealed class GetMessageAttachmentsQueryHandler
    : IRequestHandler<
        GetMessageAttachmentsQuery,
        IReadOnlyList<GetMessageAttachmentsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetMessageAttachmentsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GetMessageAttachmentsResponse>> Handle(
        GetMessageAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var message = await _context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.MessageId,
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

        return await _context.MessageAttachments
            .AsNoTracking()
            .Where(x => x.MessageId == request.MessageId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new GetMessageAttachmentsResponse(
                x.Id,
                x.MessageId,
                x.StoredFileId,
                x.Caption,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}