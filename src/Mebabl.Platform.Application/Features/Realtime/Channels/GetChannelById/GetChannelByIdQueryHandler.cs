
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
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Authenticated application credentials are required.");
        }

        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid channel ID is required.",
                nameof(request.Id));
        }

        var channel = await _context.Channels
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (channel is null)
        {
            throw new KeyNotFoundException(
                "Channel not found in this application.");
        }

        return new GetChannelByIdResponse(
            channel.Id,
            channel.Name,
            channel.IsActive,
            channel.CreatedAt);
    }
}
