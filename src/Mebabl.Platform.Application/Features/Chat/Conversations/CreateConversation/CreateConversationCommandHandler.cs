using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Modules.Chat.Entities;

namespace Mebabl.Platform.Application.Features.Chat.Conversations.CreateConversation;

public sealed class CreateConversationCommandHandler
    : IRequestHandler<
        CreateConversationCommand,
        CreateConversationResponse>
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
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var conversation = new Conversation
        {
            ApplicationId = _currentApplication.ApplicationId,
            Title = request.Title,
            IsGroup = request.IsGroup
        };

        _context.Conversations.Add(conversation);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateConversationResponse(
            conversation.Id,
            conversation.Title,
            conversation.IsGroup,
            conversation.CreatedAt);
    }
}