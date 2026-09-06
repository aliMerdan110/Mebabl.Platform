
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Live.Enums;

namespace Mebabl.Platform.Application.Features.Live.Streams.GetStreams;

public sealed class GetStreamsQueryHandler
    : IRequestHandler<GetStreamsQuery, GetStreamsResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetStreamsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<GetStreamsResponse> Handle(
        GetStreamsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var applicationId = _currentUser.ApplicationId;

        var streams = await _dbContext.LiveStreams
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new GetStreamsItem(
                x.Id,
                x.Name,
                x.Title,
                x.Description,
                $"https://live.mebabl.com/hls/{x.Id}.m3u8",
                x.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new GetStreamsResponse(streams);
    }
}
