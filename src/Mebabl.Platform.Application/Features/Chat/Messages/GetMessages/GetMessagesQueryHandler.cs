using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessages;

public sealed class GetMessagesQueryHandler
    : IRequestHandler<
        GetMessagesQuery,
        IReadOnlyList<GetMessagesResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetMessagesQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GetMessagesResponse>> Handle(
        GetMessagesQuery request,
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
            throw new UnauthorizedAccessException();

        return await _context.Messages
    .AsNoTracking()
    .Where(x =>
        x.ConversationId == request.ConversationId &&
        !x.IsDeleted)
    .OrderByDescending(x => x.CreatedAt)
    .Skip(request.Offset)
    .Take(request.Limit)
    .Select(x => new GetMessagesResponse(
        x.Id,
        x.ConversationId,
        x.SenderId,
        x.Content,
        x.MessageType,
        x.IsEdited,
        x.EditedAt,
        x.CreatedAt))
    .ToListAsync(cancellationToken);
    }
}