using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;
using Mebabl.Platform.Domain.Entities.Social;

namespace Mebabl.Platform.Application.Features.SdkSocial.Shares.CreateShare;

public sealed class ShareCommandHandler
    : IRequestHandler<ShareCommand, ShareDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public ShareCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<ShareDto> Handle(
        ShareCommand request,
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

        var share = new SocialShare
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            PostId = request.PostId,
            UserId = userId
        };

        _db.SocialShares.Add(share);

        await _db.SaveChangesAsync(cancellationToken);

        return new ShareDto(
            share.Id,
            share.PostId,
            share.UserId,
            share.CreatedAt);
    }
}