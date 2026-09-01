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
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await _dbContext.Roles
            .AsNoTracking()
            .Where(x => x.ApplicationId == _currentApplication.ApplicationId)
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