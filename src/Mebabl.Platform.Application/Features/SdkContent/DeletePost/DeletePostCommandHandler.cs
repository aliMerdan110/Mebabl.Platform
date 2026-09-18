using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.SdkContent.DeletePost;

public sealed class DeletePostCommandHandler
    : IRequestHandler<DeletePostCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public DeletePostCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task Handle(
        DeletePostCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;
        var userId = _currentUser.UserId;

        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "Authenticated user is required.");

        var post = await _db.Posts
            .FirstOrDefaultAsync(x =>
                x.Id == request.PostId &&
                x.ApplicationId == applicationId &&
                x.OwnerId == userId &&
                !x.IsDeleted,
                cancellationToken);

        if (post is null)
            throw new KeyNotFoundException("Post not found.");

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}