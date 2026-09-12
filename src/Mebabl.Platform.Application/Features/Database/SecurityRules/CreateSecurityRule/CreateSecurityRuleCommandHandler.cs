using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;

public sealed class CreateSecurityRuleCommandHandler
    : IRequestHandler<CreateSecurityRuleCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public CreateSecurityRuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(
        CreateSecurityRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.CollectionId &&
                    x.ApplicationId == _currentUser.ApplicationId &&
                    x.IsActive,
                cancellationToken);

        if (collection is null)
            throw new KeyNotFoundException("Collection not found.");

        var permission = request.Permission.Trim().ToLowerInvariant();

        var validPermissions = new[]
        {
            "read",
            "write",
            "delete",
            "query"
        };

        if (!validPermissions.Contains(permission))
            throw new ArgumentException("Invalid permission.");

        var exists = await _dbContext.SecurityRules
            .AnyAsync(
                x =>
                    x.CollectionId == request.CollectionId &&
                    x.Permission == permission &&
                    x.IsActive,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                "Security rule already exists.");

        var rule = new SecurityRule
        {
            Id = Guid.NewGuid(),
            CollectionId = request.CollectionId,
            Permission = permission,
            CanRead = request.CanRead,
            CanWrite = request.CanWrite,
            CanDelete = request.CanDelete,
            CanQuery = request.CanQuery,
            IsActive = true
        };

        _dbContext.SecurityRules.Add(rule);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }
}