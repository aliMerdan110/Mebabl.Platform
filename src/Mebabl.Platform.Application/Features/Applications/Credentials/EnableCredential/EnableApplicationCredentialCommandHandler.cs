
using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.EnableCredential;

public sealed class EnableApplicationCredentialCommandHandler
    : IRequestHandler<EnableApplicationCredentialCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public EnableApplicationCredentialCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        EnableApplicationCredentialCommand request,
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

        credential.IsActive = true;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
