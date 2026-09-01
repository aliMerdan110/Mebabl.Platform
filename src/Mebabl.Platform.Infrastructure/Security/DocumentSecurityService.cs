using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;

namespace Mebabl.Platform.Infrastructure.Security;

public sealed class DocumentSecurityService
    : IDocumentSecurityService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;


    public DocumentSecurityService(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
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
        var allowed =
            await _dbContext.SecurityRules
            .AnyAsync(
                x =>
                    x.CollectionId == collectionId &&
                    x.Permission == permission &&
                    x.IsActive,
                cancellationToken);


        if (!allowed)
            throw new UnauthorizedAccessException();
    }
}