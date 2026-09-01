using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.RemoveParticipant;

public sealed class RemoveParticipantCommandHandler
    : IRequestHandler<RemoveParticipantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public RemoveParticipantCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        RemoveParticipantCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var currentParticipant =
            await _context.ConversationParticipants
                .FirstOrDefaultAsync(
                    x =>
                        x.ConversationId == request.ConversationId &&
                        x.UserId == _currentUser.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (currentParticipant is null)
            throw new UnauthorizedAccessException(
                "You are not a participant.");

        if (!currentParticipant.IsAdmin)
            throw new UnauthorizedAccessException(
                "Only conversation admins can remove participants.");

        var targetParticipant =
            await _context.ConversationParticipants
                .FirstOrDefaultAsync(
                    x =>
                        x.ConversationId == request.ConversationId &&
                        x.UserId == request.UserId &&
                        x.LeftAt == null,
                    cancellationToken);

        if (targetParticipant is null)
            throw new KeyNotFoundException(
                "Participant not found.");

        targetParticipant.LeftAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}