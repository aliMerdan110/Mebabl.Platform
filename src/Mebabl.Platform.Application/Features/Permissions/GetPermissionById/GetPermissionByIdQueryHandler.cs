using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Permissions.GetPermissionById;

public sealed class GetPermissionByIdQueryHandler
    : IRequestHandler<GetPermissionByIdQuery, GetPermissionByIdResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public GetPermissionByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<GetPermissionByIdResponse> Handle(
        GetPermissionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application is required.");
        }

        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid permission ID is required.",
                nameof(request.Id));
        }

        var permission = await _dbContext.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (permission is null)
            throw new KeyNotFoundException("Permission not found.");

        return new GetPermissionByIdResponse(
            permission.Id,
            permission.Name,
            permission.Code,
            permission.Description,
            permission.Category,
            permission.IsActive,
            permission.CreatedAt,
            permission.UpdatedAt);
    }
}