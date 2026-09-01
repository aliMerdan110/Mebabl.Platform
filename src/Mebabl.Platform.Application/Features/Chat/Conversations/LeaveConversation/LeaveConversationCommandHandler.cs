using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.LeaveConversation;

public sealed class LeaveConversationCommandHandler
    : IRequestHandler<LeaveConversationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public LeaveConversationCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        LeaveConversationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var participant =
            await _context.ConversationParticipants
                .FirstOrDefaultAsync(
                    x =>
                        x.ConversationId == request.ConversationId &&
                        x.UserId == _currentUser.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (participant is null)
            throw new KeyNotFoundException(
                "Conversation membership not found.");

        participant.LeftAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}