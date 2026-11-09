
using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.DisableCredential;

public sealed class DisableApplicationCredentialCommandHandler
    : IRequestHandler<DisableApplicationCredentialCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public DisableApplicationCredentialCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        DisableApplicationCredentialCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");

        var credential = await _dbContext.ApplicationCredentials
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.CredentialId &&
                    x.ApplicationId == request.ApplicationId &&
                    x.Application.Project.DeveloperId ==
                        _currentDeveloper.DeveloperId &&
                    !x.Application.IsDeleted,
                cancellationToken);

        if (credential is null)
            throw new KeyNotFoundException("Credential not found.");

        credential.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
