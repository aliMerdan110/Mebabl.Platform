using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.AddParticipant;

public sealed class AddParticipantCommandHandler
    : IRequestHandler<AddParticipantCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public AddParticipantCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        AddParticipantCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ConversationId &&
                    x.ApplicationId ==
                        _currentApplication.ApplicationId,
                cancellationToken);

        if (conversation is null)
            throw new KeyNotFoundException(
                "Conversation not found.");

        var exists = await _context.ConversationParticipants
            .AnyAsync(
                x =>
                    x.ConversationId == request.ConversationId &&
                    x.UserId == request.UserId,
                cancellationToken);

        if (exists)
            return;

        var participant = new ConversationParticipant
        {
            ConversationId = request.ConversationId,
            UserId = request.UserId,
            JoinedAt = DateTime.UtcNow,
            IsAdmin = request.IsAdmin
        };

        _context.ConversationParticipants.Add(participant);

        await _context.SaveChangesAsync(cancellationToken);
    }
}