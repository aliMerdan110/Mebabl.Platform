using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.CreateConversation;

public sealed class CreateConversationCommandHandler
    : IRequestHandler<CreateConversationCommand, CreateConversationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public CreateConversationCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<CreateConversationResponse> Handle(
        CreateConversationCommand request,
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

        if (request.Title?.Length > 200)
        {
            throw new ArgumentException(
                "Conversation title cannot exceed 200 characters.",
                nameof(request.Title));
        }

        var userIsActive = await _context.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == userId &&
                    x.ApplicationId == applicationId &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    !x.Account.IsDeleted &&
                    x.Application.IsActive &&
                    !x.Application.IsDeleted,
                cancellationToken);

        if (!userIsActive)
        {
            throw new UnauthorizedAccessException(
                "The user account is inactive or unavailable.");
        }

        var now = DateTime.UtcNow;

        var conversation = new Conversation
        {
            ApplicationId = applicationId,
            Title = string.IsNullOrWhiteSpace(request.Title)
                ? null
                : request.Title.Trim(),
            IsGroup = request.IsGroup
        };

        // The creator must be an active participant and conversation admin.
        conversation.Participants.Add(
            new ConversationParticipant
            {
                UserId = userId,
                JoinedAt = now,
                IsAdmin = true
            });

        _context.Conversations.Add(conversation);

        // Persist the conversation and its initial participant together.
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateConversationResponse(
            conversation.Id,
            conversation.Title,
            conversation.IsGroup,
            conversation.CreatedAt);
    }
}