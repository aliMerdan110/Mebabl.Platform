using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.GetConversations;

public sealed class GetConversationsQueryHandler
    : IRequestHandler<GetConversationsQuery, IReadOnlyList<GetConversationsResponse>>
{
    private const int MaxPageSize = 100;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetConversationsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<GetConversationsResponse>> Handle(
        GetConversationsQuery request,
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

        return await _context.Conversations
            .AsNoTracking()
            .Where(
                x =>
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted &&
                    x.Participants.Any(
                        p =>
                            p.UserId == userId &&
                            p.LeftAt == null))
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip(request.Offset)
            .Take(request.Limit)
            .Select(
                x => new GetConversationsResponse(
                    x.Id,
                    x.Title,
                    x.IsGroup,
                    x.CreatedAt,
                    x.Participants
                        .Where(p => p.LeftAt == null)
                        .OrderBy(p => p.JoinedAt)
                        .Select(p => p.UserId)
                        .ToList()))
            .ToListAsync(cancellationToken);
    }
}