
using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Messages.GetMessages;

public sealed class GetMessagesQueryHandler
    : IRequestHandler<
        GetMessagesQuery,
        IReadOnlyList<GetMessagesResponse>>
{
    private const int MaxPageSize = 100;

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

        if (request.ConversationId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid conversation ID is required.",
                nameof(request.ConversationId));
        }

        if (request.Offset < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Offset),
                "Offset cannot be negative.");
        }

        if (request.Limit < 1 || request.Limit > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Limit),
                $"Limit must be between 1 and {MaxPageSize}.");
        }

        var isParticipant = await _context.ConversationParticipants
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ConversationId == request.ConversationId &&
                    x.UserId == userId &&
                    x.LeftAt == null &&
                    x.Conversation.ApplicationId == applicationId &&
                    !x.Conversation.IsDeleted,
                cancellationToken);

        if (!isParticipant)
        {
            throw new KeyNotFoundException(
                "Conversation not found or access is unavailable.");
        }

        return await _context.Messages
            .AsNoTracking()
            .Where(x =>
                x.ConversationId == request.ConversationId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
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
