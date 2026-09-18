using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reactions.React;

public sealed class ReactCommandHandler
    : IRequestHandler<ReactCommand, ReactionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public ReactCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<ReactionDto> Handle(
        ReactCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException();

        var postExists = await _db.Posts.AnyAsync(
            x => x.Id == request.PostId &&
                 x.ApplicationId == applicationId &&
                 !x.IsDeleted,
            cancellationToken);

        if (!postExists)
            throw new KeyNotFoundException("Post not found.");

        var type = string.IsNullOrWhiteSpace(request.Type)
            ? "Like"
            : request.Type.Trim();

        var reaction = await _db.SocialReactions
            .FirstOrDefaultAsync(
                x => x.PostId == request.PostId &&
                     x.UserId == userId &&
                     x.ApplicationId == applicationId,
                cancellationToken);

        if (reaction is null)
        {
            reaction = new SocialReaction
            {
                Id = Guid.NewGuid(),
                ApplicationId = applicationId,
                PostId = request.PostId,
                UserId = userId,
                Type = type,
                CreatedAt = DateTime.UtcNow
            };

            _db.SocialReactions.Add(reaction);
        }
        else
        {
            reaction.Type = type;
            reaction.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new ReactionDto(
            reaction.Id,
            reaction.PostId,
            reaction.UserId,
            reaction.Type,
            reaction.CreatedAt);
    }
}