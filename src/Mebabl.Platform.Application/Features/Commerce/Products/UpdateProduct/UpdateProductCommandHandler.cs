using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Commerce.Products.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentApplication _currentApplication;
    private readonly ICurrentUser _currentUser;

    public UpdateProductCommandHandler(
        IApplicationDbContext db,
        ICurrentApplication currentApplication,
        ICurrentUser currentUser)
    {
        _db = db;
        _currentApplication = currentApplication;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(
        UpdateProductCommand request,
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

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Currency = request.Currency.Trim().ToUpperInvariant();
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}