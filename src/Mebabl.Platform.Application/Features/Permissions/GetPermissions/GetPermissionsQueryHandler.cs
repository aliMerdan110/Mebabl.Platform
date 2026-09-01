using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Permissions.GetPermissions;

public sealed class GetPermissionsQueryHandler
    : IRequestHandler<GetPermissionsQuery, IReadOnlyList<PermissionListItem>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public GetPermissionsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<PermissionListItem>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await _dbContext.Permissions
            .AsNoTracking()
            .Where(x => x.ApplicationId == _currentApplication.ApplicationId)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .Select(x => new PermissionListItem(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.Category,
                x.IsActive,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}