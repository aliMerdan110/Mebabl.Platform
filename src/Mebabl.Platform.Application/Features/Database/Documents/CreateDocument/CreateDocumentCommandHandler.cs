using MediatR;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;

public sealed class CreateDocumentCommandHandler
    : IRequestHandler<CreateDocumentCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;

    public CreateDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _security = security;
    }

    public async Task<Guid> Handle(
        CreateDocumentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty ||
            _currentUser.UserId == Guid.Empty)
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

        await _security.EnsureWriteAsync(
            request.CollectionId,
            cancellationToken);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            ApplicationId = _currentUser.ApplicationId,
            CollectionId = request.CollectionId,
            UserId = request.UserId ?? _currentUser.UserId,
            Key = request.Key.Trim(),
            Data = JsonDocument.Parse(
                request.Data.RootElement.GetRawText()),
            Version = 1,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Documents.Add(document);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return document.Id;
    }
}