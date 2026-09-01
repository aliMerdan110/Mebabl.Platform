using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.UpdateChannel;

public sealed class UpdateChannelCommandHandler
    : IRequestHandler<UpdateChannelCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public UpdateChannelCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        UpdateChannelCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var channel = await _context.Channels
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == _currentApplication.ApplicationId,
                cancellationToken);

        if (channel is null)
            throw new Exception("Channel not found.");

        channel.Name = request.Name;
        channel.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}