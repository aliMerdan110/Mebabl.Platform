using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Roles.GetRoleById;

public sealed class GetRoleByIdQueryHandler
    : IRequestHandler<GetRoleByIdQuery, GetRoleByIdResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public GetRoleByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<GetRoleByIdResponse> Handle(
        GetRoleByIdQuery request,
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
                "A valid role ID is required.",
                nameof(request.Id));
        }

        var role = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (role is null)
            throw new KeyNotFoundException("Role not found.");

        return new GetRoleByIdResponse(
            role.Id,
            role.Name,
            role.Code,
            role.Description,
            role.IsActive,
            role.CreatedAt,
            role.UpdatedAt);
    }
}