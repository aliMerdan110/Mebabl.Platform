using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Realtime;

namespace Mebabl.Platform.Application.Features.Realtime.Channels.CreateChannel;

public sealed class CreateChannelCommandHandler
    : IRequestHandler<CreateChannelCommand, CreateChannelResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public CreateChannelCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<CreateChannelResponse> Handle(
        CreateChannelCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Authenticated application credentials are required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException(
                "Channel name is required.",
                nameof(request.Name));
        }

        var channel = new Channel
        {
            ApplicationId = applicationId,
            Name = request.Name.Trim(),
            IsActive = true
        };

        _context.Channels.Add(channel);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateChannelResponse(
            channel.Id,
            channel.Name);
    }
}