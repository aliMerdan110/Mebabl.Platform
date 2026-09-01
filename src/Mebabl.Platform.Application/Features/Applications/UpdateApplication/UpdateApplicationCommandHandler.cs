using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.UpdateApplication;

public sealed class UpdateApplicationHandler
    : IRequestHandler<UpdateApplicationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public UpdateApplicationHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        UpdateApplicationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (application is null)
            throw new Exception("Application not found.");

        application.Name = request.Name;
        application.Description = request.Description;
        application.Domain = request.Domain;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}