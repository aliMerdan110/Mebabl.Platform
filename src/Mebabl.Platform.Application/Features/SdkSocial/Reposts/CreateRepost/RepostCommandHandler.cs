using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reposts.CreateRepost;

public sealed class RepostCommandHandler
    : IRequestHandler<RepostCommand, RepostDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public RepostCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<RepostDto> Handle(
        RepostCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var postExists = await _db.Posts
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.PostId &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (!postExists)
            throw new KeyNotFoundException("Post not found.");

        var existing = await _db.SocialReposts
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.PostId == request.PostId &&
                    x.UserId == userId,
                cancellationToken);

        if (existing is not null)
        {
            return new RepostDto(
                existing.Id,
                existing.PostId,
                existing.UserId,
                existing.CreatedAt);
        }

        var repost = new SocialRepost
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            PostId = request.PostId,
            UserId = userId
        };

        _db.SocialReposts.Add(repost);

        await _db.SaveChangesAsync(cancellationToken);

        return new RepostDto(
            repost.Id,
            repost.PostId,
            repost.UserId,
            repost.CreatedAt);
    }
}