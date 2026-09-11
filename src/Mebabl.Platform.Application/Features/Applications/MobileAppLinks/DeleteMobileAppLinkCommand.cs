using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.MobileAppLinks;

public sealed record DeleteMobileAppLinkCommand(
    Guid ApplicationId,
    Guid Id
) : IRequest;

public sealed class DeleteMobileAppLinkCommandHandler
    : IRequestHandler<DeleteMobileAppLinkCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;

    public DeleteMobileAppLinkCommandHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        DeleteMobileAppLinkCommand request,
        CancellationToken cancellationToken)
    {
        var applicationExists = await _db.Applications
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (!applicationExists)
            throw new KeyNotFoundException(
                "Application not found.");

        var entity = await _db.ApplicationMobileAppLinks
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == request.ApplicationId,
                cancellationToken);

        if (entity is null)
            throw new KeyNotFoundException(
                "Mobile app link not found.");

        _db.ApplicationMobileAppLinks.Remove(entity);

        await _db.SaveChangesAsync(cancellationToken);
    }
}