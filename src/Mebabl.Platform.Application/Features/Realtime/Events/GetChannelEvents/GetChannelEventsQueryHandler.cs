using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Realtime.Events.GetChannelEvents;

public sealed class GetChannelEventsQueryHandler
    : IRequestHandler<
        GetChannelEventsQuery,
        IReadOnlyList<GetChannelEventsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public GetChannelEventsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<GetChannelEventsResponse>> Handle(
        GetChannelEventsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var channelExists = await _context.Channels
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.ChannelId &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (!channelExists)
            throw new Exception("Channel not found.");

        return await _context.RealtimeEvents
            .AsNoTracking()
            .Where(x => x.ChannelId == request.ChannelId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(request.Offset)
            .Take(request.Limit)
            .Select(x => new GetChannelEventsResponse(
                x.Id,
                x.ChannelId,
                x.Name,
                x.Payload,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}