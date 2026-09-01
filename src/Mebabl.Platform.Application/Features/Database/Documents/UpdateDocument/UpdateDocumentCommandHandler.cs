using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

public sealed class UpdateDocumentCommandHandler
    : IRequestHandler<UpdateDocumentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IDocumentSecurityService _security;


    public UpdateDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _security = security;
    }


    public async Task Handle(
        UpdateDocumentCommand request,
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


        await _security.EnsureWriteAsync(
            document.CollectionId,
            cancellationToken);


        document.Data = request.Data;
        document.Version++;


        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}