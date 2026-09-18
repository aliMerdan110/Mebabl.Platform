using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.SdkSocial.DTOs;

namespace Mebabl.Platform.Application.Features.SdkSocial.Reactions.GetReactions;

public sealed class GetReactionsQueryHandler
    : IRequestHandler<GetReactionsQuery, IReadOnlyList<ReactionDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public GetReactionsQueryHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<ReactionDto>> Handle(
        GetReactionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _db.SocialReactions
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.PostId == request.PostId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ReactionDto(
                x.Id,
                x.PostId,
                x.UserId,
                x.Type,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}