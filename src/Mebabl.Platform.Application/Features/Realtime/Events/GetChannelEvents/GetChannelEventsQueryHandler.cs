using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Realtime.Events.GetChannelEvents;

public sealed class GetChannelEventsQueryHandler
    : IRequestHandler<
        GetChannelEventsQuery,
        IReadOnlyList<GetChannelEventsResponse>>
{
    private const int MaxPageSize = 100;

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
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Authenticated application credentials are required.");
        }

        if (request.ChannelId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid channel ID is required.",
                nameof(request.ChannelId));
        }

        if (request.Offset < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Offset),
                "Offset cannot be negative.");
        }

        if (request.Limit < 1 || request.Limit > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Limit),
                $"Limit must be between 1 and {MaxPageSize}.");
        }

        var channelExists = await _context.Channels
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.ChannelId &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (!channelExists)
        {
            throw new KeyNotFoundException(
                "Channel not found in this application.");
        }

        return await _context.RealtimeEvents
            .AsNoTracking()
            .Where(x => x.ChannelId == request.ChannelId)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
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