using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

public sealed record DeleteMobileAppLinkCommand(Guid Id) : IRequest;

public sealed class DeleteMobileAppLinkCommandHandler
    : IRequestHandler<DeleteMobileAppLinkCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;

    public DeleteMobileAppLinkCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication)
    {
        _db = db;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        DeleteMobileAppLinkCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        var entity = await _db.ApplicationMobileAppLinks
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.ApplicationId == applicationId,
                cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException("Mobile app link not found.");

        _db.ApplicationMobileAppLinks.Remove(entity);

        await _db.SaveChangesAsync(cancellationToken);
    }
}