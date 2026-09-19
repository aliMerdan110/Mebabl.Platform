using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Products.DeleteProduct;

public sealed class DeleteProductCommandHandler
    : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public DeleteProductCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(x =>
                x.Id == request.ProductId &&
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.OwnerId == _currentUser.UserId,
                cancellationToken);

        if (product is null)
            return false;

        _db.Products.Remove(product);

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}