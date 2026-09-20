using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.PostStats.GetPostStats;

public sealed class GetPostStatsQueryHandler
    : IRequestHandler<GetPostStatsQuery, IReadOnlyList<PostSocialStatsDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public GetPostStatsQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<PostSocialStatsDto>> Handle(
        GetPostStatsQuery request,
        CancellationToken cancellationToken)
    {
        var postIds = request.PostIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (postIds.Length == 0)
            return Array.Empty<PostSocialStatsDto>();

        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        var likes = await _db.SocialReactions
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                postIds.Contains(x.PostId))
            .GroupBy(x => x.PostId)
            .Select(x => new
            {
                PostId = x.Key,
                Count = x.Count()
            })
            .ToDictionaryAsync(
                x => x.PostId,
                x => x.Count,
                cancellationToken);

        var comments = await _db.SocialComments
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                postIds.Contains(x.PostId))
            .GroupBy(x => x.PostId)
            .Select(x => new
            {
                PostId = x.Key,
                Count = x.Count()
            })
            .ToDictionaryAsync(
                x => x.PostId,
                x => x.Count,
                cancellationToken);

        var shares = await _db.SocialShares
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                postIds.Contains(x.PostId))
            .GroupBy(x => x.PostId)
            .Select(x => new
            {
                PostId = x.Key,
                Count = x.Count()
            })
            .ToDictionaryAsync(
                x => x.PostId,
                x => x.Count,
                cancellationToken);

        var reposts = await _db.SocialReposts
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                postIds.Contains(x.PostId))
            .GroupBy(x => x.PostId)
            .Select(x => new
            {
                PostId = x.Key,
                Count = x.Count()
            })
            .ToDictionaryAsync(
                x => x.PostId,
                x => x.Count,
                cancellationToken);

        var likedPostIds = await _db.SocialReactions
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                x.UserId == userId &&
                postIds.Contains(x.PostId))
            .Select(x => x.PostId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var repostedPostIds = await _db.SocialReposts
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                x.UserId == userId &&
                postIds.Contains(x.PostId))
            .Select(x => x.PostId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var result = postIds
            .Select(postId => new PostSocialStatsDto(
                postId,
                likes.GetValueOrDefault(postId),
                comments.GetValueOrDefault(postId),
                shares.GetValueOrDefault(postId),
                reposts.GetValueOrDefault(postId),
                likedPostIds.Contains(postId),
                repostedPostIds.Contains(postId)))
            .ToList();

        return result;
    }
}