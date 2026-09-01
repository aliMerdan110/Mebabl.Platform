using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.GetChannels;

public sealed class GetChannelsQueryHandler
    : IRequestHandler<GetChannelsQuery, IReadOnlyList<GetChannelsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public GetChannelsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }


    public async Task<IReadOnlyList<GetChannelsResponse>> Handle(
        GetChannelsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();


        return await _context.Channels
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == _currentApplication.ApplicationId)
            .Select(x => new GetChannelsResponse(
                x.Id,
                x.Name,
                x.IsActive,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}