using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Live;

namespace Mebabl.Platform.Application.Features.Live.Streams.GetStreams;

public sealed class GetStreamsQueryHandler
    : IRequestHandler<GetStreamsQuery, GetStreamsResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILiveAuthorizationService _authorization;

    public GetStreamsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        ILiveAuthorizationService authorization)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _authorization = authorization;
    }

    public async Task<GetStreamsResponse> Handle(
        GetStreamsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty ||
            _currentUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The current user is not authenticated.");
        }

        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        var allowed = await _authorization.CanViewAsync(
    applicationId,
    userId,
    Guid.Empty,
    cancellationToken);

        if (!allowed)
        {
            throw new UnauthorizedAccessException(
                "The current user is not allowed to view live streams.");
        }

        var streams = await _dbContext.LiveStreams
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new GetStreamsItem(
                x.Id,
                x.Name,
                x.Title,
                x.Description,
                $"https://live.mebabl.com/hls/{Uri.EscapeDataString(x.Name)}.m3u8",
                x.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new GetStreamsResponse(streams);
    }
}