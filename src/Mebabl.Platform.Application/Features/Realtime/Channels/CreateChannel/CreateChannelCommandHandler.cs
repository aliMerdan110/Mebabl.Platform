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
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();


        var channel = new Channel
        {
            ApplicationId = _currentApplication.ApplicationId,
            Name = request.Name,
            IsActive = true
        };


        _context.Channels.Add(channel);

        await _context.SaveChangesAsync(cancellationToken);


        return new CreateChannelResponse(
            channel.Id,
            channel.Name);
    }
}