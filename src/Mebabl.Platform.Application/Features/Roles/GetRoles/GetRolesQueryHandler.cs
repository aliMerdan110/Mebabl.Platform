using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Roles.GetRoles;

public sealed class GetRolesQueryHandler
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleListItem>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public GetRolesQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<RoleListItem>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application is required.");
        }

        return await _dbContext.Roles
            .AsNoTracking()
            .Where(x => x.ApplicationId == applicationId)
            .OrderBy(x => x.Name)
            .Select(x => new RoleListItem(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}