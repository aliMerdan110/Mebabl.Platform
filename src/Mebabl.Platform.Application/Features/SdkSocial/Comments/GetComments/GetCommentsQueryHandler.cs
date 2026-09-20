using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Comments.GetComments;

public sealed class GetCommentsQueryHandler
    : IRequestHandler<GetCommentsQuery, IReadOnlyList<CommentDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public GetCommentsQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<CommentDto>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return await _db.SocialComments
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.PostId == request.PostId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CommentDto(
                x.Id,
                x.PostId,
                x.OwnerId,
                x.ParentCommentId,
                x.Text,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}