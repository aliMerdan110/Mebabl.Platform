using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.GetChannelById;

public sealed class GetChannelByIdQueryHandler
    : IRequestHandler<GetChannelByIdQuery, GetChannelByIdResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public GetChannelByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<GetChannelByIdResponse> Handle(
        GetChannelByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var channel = await _context.Channels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (channel is null)
            throw new Exception("Channel not found.");

        return new GetChannelByIdResponse(
            channel.Id,
            channel.Name,
            channel.IsActive,
            channel.CreatedAt);
    }
}