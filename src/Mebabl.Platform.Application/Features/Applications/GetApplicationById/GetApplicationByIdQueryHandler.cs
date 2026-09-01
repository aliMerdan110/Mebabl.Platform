using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.GetApplicationById;

public sealed class GetApplicationByIdQueryHandler
    : IRequestHandler<GetApplicationByIdQuery, GetApplicationByIdResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetApplicationByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<GetApplicationByIdResponse> Handle(
        GetApplicationByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var application = await _dbContext.Applications
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (application is null)
            throw new Exception("Application not found.");

        return new GetApplicationByIdResponse(
            application.Id,
            application.Name,
            application.Code,
            application.Description,
            application.Domain,
            application.IsActive,
            application.CreatedAt);
    }
}