using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.DeleteChannel;

public sealed class DeleteChannelCommandHandler
    : IRequestHandler<DeleteChannelCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public DeleteChannelCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        DeleteChannelCommand request,
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

        channel.IsDeleted = true;
        channel.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}