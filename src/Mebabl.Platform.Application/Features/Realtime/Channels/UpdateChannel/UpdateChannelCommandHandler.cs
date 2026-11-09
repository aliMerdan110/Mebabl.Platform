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

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Channel name is required.",
                nameof(request.Name));
        }

        var channel = await _context.Channels
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

        channel.Name = request.Name.Trim();
        channel.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
}