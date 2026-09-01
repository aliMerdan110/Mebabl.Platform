using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;

namespace Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

public sealed class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IDocumentSecurityService _security;


    public DeleteDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _security = security;
    }


    public async Task Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();


        var document = await _dbContext.Documents
            .Include(x => x.Collection)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.Collection.ApplicationId ==
                    _currentApplication.ApplicationId,
                cancellationToken);


        if (document is null)
            throw new Exception("Document not found.");


        await _security.EnsureDeleteAsync(
            document.CollectionId,
            cancellationToken);


        _dbContext.Documents.Remove(document);


        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}