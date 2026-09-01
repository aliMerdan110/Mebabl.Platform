using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;

public sealed class CreateDocumentCommandHandler
    : IRequestHandler<CreateDocumentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;


    public CreateDocumentCommandHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IDocumentSecurityService security)
    {
        _context = context;
        _currentUser = currentUser;
        _security = security;
    }


    public async Task<Guid> Handle(
        CreateDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.CollectionId &&
                    x.ApplicationId == _currentUser.ApplicationId,
                cancellationToken);


        if (collection is null)
            throw new Exception("Collection not found.");


        await _security.EnsureWriteAsync(
            request.CollectionId,
            cancellationToken);



        var document = new Document
        {
            CollectionId = collection.Id,

            Key = request.Key,

            Data = JsonDocument.Parse(
                request.Data.RootElement.GetRawText())
        };


        _context.Documents.Add(document);


        await _context.SaveChangesAsync(
            cancellationToken);


        return document.Id;
    }
}