using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.Credentials.GetCredentials;

public sealed class GetApplicationCredentialsQueryHandler
    : IRequestHandler<
        GetApplicationCredentialsQuery,
        IReadOnlyList<ApplicationCredentialResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetApplicationCredentialsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<IReadOnlyList<ApplicationCredentialResponse>> Handle(
        GetApplicationCredentialsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var exists = await _dbContext.Applications.AnyAsync(
            x =>
                x.Id == request.ApplicationId &&
                x.DeveloperId == _currentDeveloper.DeveloperId,
            cancellationToken);

        if (!exists)
            throw new Exception("Application not found.");

        return await _dbContext.ApplicationCredentials
            .AsNoTracking()
            .Where(x => x.ApplicationId == request.ApplicationId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ApplicationCredentialResponse(
                x.Id,
                x.ApiKey,
                x.IsActive,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}