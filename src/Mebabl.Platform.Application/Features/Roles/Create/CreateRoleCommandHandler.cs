using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.Roles.Create;

public sealed class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public CreateRoleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
    }

    public async Task<CreateRoleResponse> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var code = request.Code
            .Trim()
            .ToUpperInvariant();

        var exists = await _dbContext.Roles
            .AnyAsync(
                x =>
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    x.Code == code,
                cancellationToken);

        if (exists)
            throw new Exception("Role code already exists.");

        var role = new Role
        {
            ApplicationId = _currentApplication.ApplicationId,
            Name = request.Name.Trim(),
            Code = code,
            Description = request.Description ?? string.Empty
        };

        _dbContext.Roles.Add(role);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateRoleResponse(
            role.Id,
            role.Name,
            role.Code);
    }
}