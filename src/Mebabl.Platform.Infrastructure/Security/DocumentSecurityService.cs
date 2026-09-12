using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Common.Security;

public sealed class DocumentSecurityService : IDocumentSecurityService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DocumentSecurityService(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public Task EnsureReadAsync(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        return CheckAsync(
            collectionId,
            "read",
            cancellationToken);
    }

    public Task EnsureWriteAsync(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        return CheckAsync(
            collectionId,
            "write",
            cancellationToken);
    }

    public Task EnsureDeleteAsync(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        return CheckAsync(
            collectionId,
            "delete",
            cancellationToken);
    }

    public Task EnsureQueryAsync(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        return CheckAsync(
            collectionId,
            "query",
            cancellationToken);
    }

    private async Task CheckAsync(
        Guid collectionId,
        string permission,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var rule = await _dbContext.SecurityRules
            .AsNoTracking()
            .Include(x => x.Collection)
            .FirstOrDefaultAsync(
                x =>
                    x.CollectionId == collectionId &&
                    x.Collection.ApplicationId ==
                        _currentUser.ApplicationId &&
                    x.Permission == permission &&
                    x.IsActive,
                cancellationToken);

        if (rule is null)
            throw new UnauthorizedAccessException();

        var allowed = permission switch
        {
            "read" => rule.CanRead,
            "write" => rule.CanWrite,
            "delete" => rule.CanDelete,
            "query" => rule.CanQuery,
            _ => false
        };

        if (!allowed)
            throw new UnauthorizedAccessException();
    }
}