using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.UpdateComment;

public sealed class UpdateCommentCommandHandler
    : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public UpdateCommentCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<CommentDto> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            throw new ArgumentException("Comment text is required.");

        var comment = await _db.SocialComments
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.CommentId &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    x.OwnerId == _currentUser.UserId,
                cancellationToken);

        if (comment is null)
            throw new KeyNotFoundException("Comment not found.");

        comment.Text = request.Text.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return new CommentDto(
            comment.Id,
            comment.PostId,
            comment.OwnerId,
            comment.ParentCommentId,
            comment.Text,
            comment.CreatedAt,
            comment.UpdatedAt);
    }
}